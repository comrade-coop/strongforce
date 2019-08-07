using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
using Action = StrongForce.Core.Action;

namespace StrongForce.Integrations.Cosmos
{
	public class StrongForceServer : Strongforce.StrongForce.StrongForceBase
	{
		private ILogger<StrongForceServer> logger;
		private Kit initialKit;

		public StrongForceServer(ILogger<StrongForceServer> logger, Kit initialKit)
		{
			this.logger = logger;
			this.initialKit = initialKit;
		}

		public override async Task ExecuteAction(IAsyncStreamReader<ActionOrContract> requestStream, IServerStreamWriter<ContractRequest> responseStream, ServerCallContext context)
		{
			var streamingToken = new CancellationTokenSource();

			var actionFinished = false;
			var pendingResponsesSemaphore = new Semaphore(0, int.MaxValue);
			var pendingTasks = new Dictionary<Address, TaskCompletionSource<Contract>>();

			var registry = new RemoteContractRegistry((address) =>
			{
				pendingTasks[address] = new TaskCompletionSource<Contract>();
				pendingResponsesSemaphore.Release();
				responseStream.WriteAsync(new ContractRequest
				{
					Address = ByteString.CopyFrom(address.Value),
				}).Wait();

				this.logger.LogTrace("Sent request for contract!");

				var result = pendingTasks[address].Task.Result;

				this.logger.LogTrace("Result from contract request received!");

				return result;
			});

			try
			{
				await requestStream.MoveNext(streamingToken.Token);

				if (requestStream.Current.Action != null)
				{
					var address = new Address(requestStream.Current.Action.Address.ToByteArray());

					var data = Encoding.UTF8.GetString(requestStream.Current.Action.Data.ToByteArray());

					this.logger.LogTrace("Received action data: " + data);

					var action = StrongForceSerialization.DeserializeAction(data);

					this.logger.LogInformation("Received an action with type: " + (action != null ? action.GetType().ToString() : "null"));

#pragma warning disable CS4014 // Awaiting will deadlock
					Task.Run(() =>
					{
						try
						{
							registry.HandleAction(address, action);
							actionFinished = true;
							pendingResponsesSemaphore.Release();
						}
						catch (Exception e)
						{
							this.logger.LogError(e.ToString());
							actionFinished = true;
							pendingResponsesSemaphore.Release();
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
					pendingResponsesSemaphore.WaitOne();
					if (actionFinished)
					{
						break;
					}

					await requestStream.MoveNext(streamingToken.Token);
					streamingToken.Token.ThrowIfCancellationRequested();

					if (requestStream.Current.Contract != null)
					{
						var address = new Address(requestStream.Current.Contract.Address.ToByteArray());

						var data = Encoding.UTF8.GetString(requestStream.Current.Contract.Data.ToByteArray());
						this.logger.LogTrace("Received contract data: " + data);

						Contract contract;

						if (data == string.Empty && address == KitContract.DefaultAddress)
						{
							this.logger.LogTrace("Fulfilling request with default kit contract!");
							contract = new KitContract(KitContract.DefaultAddress, this.initialKit);
						}
						else
						{
							contract = StrongForceSerialization.DeserializeContract(data);
						}

						this.logger.LogTrace("Received contract: " + StrongForceSerialization.SerializeContract(contract));

						this.logger.LogTrace("Received a contract with type: " + (contract != null ? contract.GetType().ToString() : "null"));

						pendingTasks[address].SetResult(contract);
					}
					else
					{
						throw new InvalidOperationException("Expected a Contract response");
					}
				}

				streamingToken.Token.ThrowIfCancellationRequested();

				this.logger.LogTrace("Request finished, starting saving!");

				foreach (var contract in registry.GetUsedContracts())
				{
					var data = StrongForceSerialization.SerializeContract(contract);

					await responseStream.WriteAsync(new ContractRequest
					{
						Address = ByteString.CopyFrom(contract.Address.Value),
						Data = ByteString.CopyFrom(Encoding.UTF8.GetBytes(data)),
					});

					this.logger.LogTrace("Saved contract #" + contract.Address.ToBase64String());
				}
			}
			catch (OperationCanceledException)
			{
				// Already logged
				throw;
			}
			catch (Exception e)
			{
				this.logger.LogError(e.ToString());
				throw;
			}
		}
	}
}