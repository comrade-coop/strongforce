using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ContractsCore;
using ContractsCore.Actions;
using ContractsCore.Contracts;
using Google.Protobuf;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Strongforce;
using Action = ContractsCore.Actions.Action;

namespace Tendermint
{
	public class StrongForceServer : StrongForce.StrongForceBase
	{
		private JsonSerializerSettings actionSerializationSettings = new JsonSerializerSettings()
		{
			SerializationBinder = new FilteredSerializationBinder()
			{
				WhitelistedBaseTypes = new HashSet<Type> { typeof(Action), typeof(Address) },
				BlacklistedTypes = new HashSet<Type> { typeof(TracingBulletAction) },
			},
			TypeNameHandling = TypeNameHandling.Auto,
			Converters = new List<JsonConverter> { new AddressJsonConverter() },
		};

		private JsonSerializerSettings contractSerializationSettings = new JsonSerializerSettings()
		{
			TypeNameHandling = TypeNameHandling.All,
		};

		private ILogger<StrongForceServer> logger;

		public StrongForceServer(ILogger<StrongForceServer> logger)
		{
			this.logger = logger;
		}

		public byte[] SerializeAction(Action action)
		{
			var serialized = JsonConvert.SerializeObject(action, typeof(Action), this.actionSerializationSettings);
			return Encoding.UTF8.GetBytes(serialized);
		}

		public Action DeserializeAction(byte[] serializedAction)
		{
			var serialized = Encoding.UTF8.GetString(serializedAction);
			return JsonConvert.DeserializeObject<Action>(serialized, this.actionSerializationSettings);
		}

		public byte[] SerializeContract(Contract contract)
		{
			var serialized = JsonConvert.SerializeObject(contract, typeof(Contract), this.contractSerializationSettings);
			return Encoding.UTF8.GetBytes(serialized);
		}

		public Contract DeserializeContract(byte[] serializedContract)
		{
			var serialized = Encoding.UTF8.GetString(serializedContract);
			return JsonConvert.DeserializeObject<Contract>(serialized, this.contractSerializationSettings);
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

						var action = this.DeserializeAction(actionOrContract.Action.Data.ToByteArray());

						this.logger.LogInformation("Received an action with type: ", action != null ? action.GetType().ToString() : "null");

#pragma warning disable CS4014 // Awaiting will deadlock
						Task.Run(() =>
						{
							try
							{
								registry.SendAction(address, action);
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

						var contract = this.DeserializeContract(actionOrContract.Contract.Data.ToByteArray());

						this.logger.LogTrace("Received a contract with type: ", contract != null ? contract.GetType().ToString() : "null");

						pendingTasks[address].SetResult(contract);
					}
				}

				foreach (var contract in registry.GetUsedContracts())
				{
					var data = this.SerializeContract(contract);

					await responseStream.WriteAsync(new ContractRequest
					{
						Address = ByteString.CopyFrom(contract.Address.Value),
						Data = ByteString.CopyFrom(data),
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