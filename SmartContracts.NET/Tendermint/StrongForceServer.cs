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
using StrongForce.Core.Permissions;
using StrongForce.Core.Serialization;
using Action = StrongForce.Core.Action;

namespace Tendermint
{
	public class StrongForceServer : Strongforce.StrongForce.StrongForceBase
	{
		private ILogger<StrongForceServer> logger;

		public StrongForceServer(ILogger<StrongForceServer> logger)
		{
			this.logger = logger;
		}

		public override async Task ExecuteAction(IAsyncStreamReader<ActionOrContract> requestStream, IServerStreamWriter<ContractRequest> responseStream, ServerCallContext context)
		{
			try
			{
				var pendingTasks = new Dictionary<Address, TaskCompletionSource<Contract>>();

				var registry = new RemoteContractRegistry((address) =>
				{
					pendingTasks[address] = new TaskCompletionSource<Contract>();
					responseStream.WriteAsync(new ContractRequest
					{
						Address = ByteString.CopyFrom(address.Value),
					}).Wait();

					this.logger.LogTrace("Sent request for contract!");

					var result = pendingTasks[address].Task.Result;

					this.logger.LogTrace("Result from contract request received!");

					return result;
				});

				var cancelRequest = new CancellationTokenSource();

				while (await requestStream.MoveNext(cancelRequest.Token))
				{
					cancelRequest.Token.ThrowIfCancellationRequested();
					var actionOrContract = requestStream.Current;
					if (actionOrContract.Action != null)
					{
						var address = new Address(actionOrContract.Action.Address.ToByteArray());

						var data = Encoding.Unicode.GetString(actionOrContract.Action.Data.ToByteArray());
						var action = StrongForceSerialization.DeserializeAction(data);

						this.logger.LogInformation("Received an action with type: ", action != null ? action.GetType().ToString() : "null");

#pragma warning disable CS4014 // Awaiting will deadlock
						Task.Run(() =>
						{
							try
							{
								registry.HandleAction(address, action);
							}
							catch (Exception e)
							{
								this.logger.LogError(e.ToString());
								cancelRequest.Cancel();
							}
						});
#pragma warning restore CS4014
					}
					else if (actionOrContract.Contract != null)
					{
						var address = new Address(actionOrContract.Contract.Address.ToByteArray());

						var data = Encoding.Unicode.GetString(actionOrContract.Contract.Data.ToByteArray());
						var contract = StrongForceSerialization.DeserializeContract(data);

						this.logger.LogTrace("Received a contract with type: ", contract != null ? contract.GetType().ToString() : "null");

						pendingTasks[address].SetResult(contract);
					}
				}

				foreach (var contract in registry.GetUsedContracts())
				{
					var data = StrongForceSerialization.SerializeContract(contract);

					await responseStream.WriteAsync(new ContractRequest
					{
						Address = ByteString.CopyFrom(contract.Address.Value),
						Data = ByteString.CopyFrom(Encoding.Unicode.GetBytes(data)),
					});

					this.logger.LogTrace("Saved contract #", contract.Address.ToBase64String());
				}
			}
			catch (OperationCanceledException)
			{
				// Already logged, pass
			}
			catch (Exception e)
			{
				this.logger.LogError(e.ToString());
			}
		}
	}
}