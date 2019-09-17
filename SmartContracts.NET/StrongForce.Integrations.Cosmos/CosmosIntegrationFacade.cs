using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Strongforce;
using StrongForce.Core;
using StrongForce.Core.Kits;
using StrongForce.Core.Permissions;
using StrongForce.Core.Serialization;
using Action = System.Action;

namespace StrongForce.Integrations.Cosmos
{
	public class CosmosIntegrationFacade : Strongforce.StrongForce.StrongForceBase, IIntegrationFacade
	{
		private Address contractRegistryAddress = new Address(new byte[] { 123, 32, 4 });
		private ILogger<CosmosIntegrationFacade> logger;
		private byte[] initialKitFallback;
		private Task currentRequestTask = Task.FromResult(true);
		private Func<ContractRequest, Task> requestDelegate;
		private Semaphore pendingResponsesSemaphore = new Semaphore(0, int.MaxValue);
		private Dictionary<Address, TaskCompletionSource<byte[]>> pendingContracts = new Dictionary<Address, TaskCompletionSource<byte[]>>();

		public CosmosIntegrationFacade(ILogger<CosmosIntegrationFacade> logger, byte[] initialKitFallback)
		{
			this.logger = logger;
			this.initialKitFallback = initialKitFallback;
		}

		public event Action<Address, Address[], string, IDictionary<string, object>> ReceiveMessage;

		public event Action DropCaches;

		public (BaseContract, Action<Message>) LoadContract(Address address, ContractHandlers handlers)
		{
			this.pendingContracts[address] = new TaskCompletionSource<byte[]>();
			this.pendingResponsesSemaphore.Release();
			this.requestDelegate.Invoke(new ContractRequest
			{
				Address = ByteString.CopyFrom(address.Value),
			}).Wait();

			this.logger.LogTrace("Sent request for contract!");

			var result = this.pendingContracts[address].Task.Result;

			this.logger.LogTrace("Result from contract request received!");

			if (result.Length == 0 && address == KitContract.DefaultAddress)
			{
				result = this.initialKitFallback;
			}

			if (result.Length == 0)
			{
				return (new Contract(), (message) => { });
			}

			return StrongForceSerialization.DeserializeContract(address, handlers, result);
		}

		public void SaveContract(BaseContract contract)
		{
			var data = StrongForceSerialization.SerializeContract(contract);

			if (contract.Address == null)
			{
				return;
			}

			this.requestDelegate.Invoke(new ContractRequest
			{
				Address = ByteString.CopyFrom(contract.Address.Value),
				Data = ByteString.CopyFrom(data),
			}).Wait();

			this.logger.LogTrace("Saved contract " + contract.Address);
		}

		public ContractRegistryState LoadRegistryState()
		{
			this.pendingContracts[this.contractRegistryAddress] = new TaskCompletionSource<byte[]>();
			this.pendingResponsesSemaphore.Release();
			this.requestDelegate.Invoke(new ContractRequest
			{
				Address = ByteString.CopyFrom(this.contractRegistryAddress.Value),
			}).Wait();

			this.logger.LogTrace("Sent request for registry state!");

			var result = this.pendingContracts[this.contractRegistryAddress].Task.Result;

			this.logger.LogTrace("Result from registry state request received!");

			if (result.Length == 0)
			{
				return new ContractRegistryState(null);
			}

			return new ContractRegistryState(StateSerialization.DeserializeState(result));
		}

		public void SaveRegistryState(ContractRegistryState registryState)
		{
			var data = StateSerialization.SerializeState(registryState.GetState());

			this.requestDelegate.Invoke(new ContractRequest
			{
				Address = ByteString.CopyFrom(this.contractRegistryAddress.Value),
				Data = ByteString.CopyFrom(data),
			}).Wait();

			this.logger.LogTrace("Saved registry state");
		}

		public override async Task ExecuteAction(IAsyncStreamReader<ActionOrContract> requestStream, IServerStreamWriter<ContractRequest> responseStream, ServerCallContext context)
		{
			var previousTask = this.currentRequestTask;

			var currentRequestTaskSource = new TaskCompletionSource<bool>();
			this.currentRequestTask = currentRequestTaskSource.Task;

			await previousTask;

			this.requestDelegate = responseStream.WriteAsync;

			var streamingToken = new CancellationTokenSource();

			var actionFinished = false;

			try
			{
				await requestStream.MoveNext(streamingToken.Token);

				if (requestStream.Current.Action != null)
				{
					var address = new Address(requestStream.Current.Action.Address.ToByteArray());

					var data = requestStream.Current.Action.Data.ToByteArray();

					this.logger.LogTrace("Received action data: " + data);

					var action = StrongForceSerialization.DeserializeAction(data);

					this.logger.LogInformation("Received an action with type: " + action.Item2);

#pragma warning disable CS4014 // Awaiting will deadlock
					Task.Run(() =>
					{
						try
						{
							// Needed since the message might fail even if we do not throw an exception
							// HACK: Would be better if the go part tells us whenever a transaction fails
							this.DropCaches.Invoke();
							this.ReceiveMessage.Invoke(address, action.Item1, action.Item2, action.Item3);
							actionFinished = true;
							this.pendingResponsesSemaphore.Release();
						}
						catch (Exception e)
						{
							// this.DropCaches.Invoke();
							this.logger.LogError(e.ToString());
							actionFinished = true;
							this.pendingResponsesSemaphore.Release();
							streamingToken.Cancel();
						}
					});
#pragma warning restore CS4014
				}
				else
				{
					throw new InvalidOperationException("Expected an Action first");
				}

				while (true)
				{
					this.pendingResponsesSemaphore.WaitOne();
					if (actionFinished)
					{
						break;
					}

					await requestStream.MoveNext(streamingToken.Token);
					streamingToken.Token.ThrowIfCancellationRequested();

					if (requestStream.Current.Contract != null)
					{
						var address = new Address(requestStream.Current.Contract.Address.ToByteArray());

						var data = requestStream.Current.Contract.Data.ToByteArray();

						this.logger.LogTrace("Received contract data: " + data.Length);

						this.pendingContracts[address].SetResult(data);
					}
					else
					{
						throw new InvalidOperationException("Expected a Contract response");
					}
				}

				streamingToken.Token.ThrowIfCancellationRequested();
			}
			catch (OperationCanceledException)
			{
				currentRequestTaskSource.SetResult(false);
				throw;
			}
			catch (Exception e)
			{
				currentRequestTaskSource.SetResult(false);
				this.logger.LogError(e.ToString());
				throw;
			}

			currentRequestTaskSource.SetResult(true);
		}
	}
}