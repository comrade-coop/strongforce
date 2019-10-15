using System;
using System.Text;
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
	public class CosmosIntegration : Strongforce.StrongForce.StrongForceBase
	{
		private ILogger<CosmosIntegration> logger;
		private Address contractRegistryAddress = new Address(new byte[] { 42 });
		private byte[] initialKitFallback;

		public CosmosIntegration(ILogger<CosmosIntegration> logger, byte[] initialKitFallback)
		{
			this.logger = logger;
			this.initialKitFallback = initialKitFallback;
		}

		public override Task ExecuteAction(IAsyncStreamReader<ActionOrContract> requestStream, IServerStreamWriter<ContractRequest> responseStream, ServerCallContext context)
		{

			var taskCompletionSource = new TaskCompletionSource<bool>();

			var thread = new Thread(() =>
			{
				try
				{
					var helper = new CosmosIntegrationHelper(requestStream, responseStream, this);
					var message = helper.ReadMessage();

					var registry = helper.LoadContractRegistry();
					var receiveMessage = registry.SetContext(helper);

					receiveMessage.Invoke(message.Sender, message.Targets, message.Type, message.Payload);

					helper.SaveContractRegistry(registry);

					taskCompletionSource.SetResult(true);
					Console.WriteLine("Hm? Done");
				}
				catch (Exception e)
				{
					Console.WriteLine("Hm? Errored");
					this.logger.LogError(e.ToString());
					taskCompletionSource.SetException(e);
				}
			});
			thread.Start();

			return taskCompletionSource.Task;
		}

		public class NonexistentContract : BaseContract
		{
			protected override void CheckPermissions(Message message)
			{
			}

			protected override void HandleMessage(Message message)
			{
			}

			protected override void HandleForwardMessage(ForwardMessage message)
			{
				throw new InvalidOperationException("Cannot forward through nonexistent contracts!");
			}
		}

		private class CosmosIntegrationHelper : IContractRegistryContext
		{
			private readonly IAsyncStreamReader<ActionOrContract> requestStream;
			private readonly IServerStreamWriter<ContractRequest> responseStream;
			private readonly CosmosIntegration integration;

			public CosmosIntegrationHelper(IAsyncStreamReader<ActionOrContract> requestStream, IServerStreamWriter<ContractRequest> responseStream, CosmosIntegration integration)
			{
				this.requestStream = requestStream;
				this.responseStream = responseStream;
				this.integration = integration;
			}

			public (Address Sender, Address[] Targets, string Type, IDictionary<string, object> Payload) ReadMessage()
			{
				var message = this.ReadFromStream();

				if (message.Action == null)
				{
					throw new InvalidOperationException("Expected an Action message");
				}

				var sender = new Address(message.Action.Address.ToByteArray());

				var data = message.Action.Data.ToByteArray();
				this.integration.logger.LogTrace("Received message data: " + data.Length + " bytes from " + sender);

				var (targets, messageType, messagePayload) = StrongForceSerialization.DeserializeMessage(data);
				this.integration.logger.LogInformation("Received an action with type: " + messageType);

				return (sender, targets, messageType, messagePayload);
			}

			public BaseContract LoadContract(Address address)
			{
				if (address == this.integration.contractRegistryAddress)
				{
					throw new ArgumentOutOfRangeException(address + " is a magic address which cannot be used for contracts.");
				}

				var data = this.LoadData(address);
				this.integration.logger.LogTrace("Received contract data: " + data.Length + " bytes for " + address);

				if (data.Length == 0)
				{
					if (address == KitContract.DefaultAddress)
					{
						data = this.integration.initialKitFallback;
					}
					else
					{
						return new NonexistentContract();
					}
				}

				return (BaseContract)StrongForceSerialization.DeserializeStatefulObject(data);
			}

			public void SaveContract(BaseContract contract)
			{
				if (contract.Address == this.integration.contractRegistryAddress)
				{
					throw new ArgumentOutOfRangeException(contract.Address + " is a magic address which cannot be used for contracts.");
				}

				if (contract is NonexistentContract)
				{
					return;
				}

				var data = StrongForceSerialization.SerializeStatefulObject(contract);
				var typeName = Encoding.ASCII.GetBytes(contract.GetType().Name);

				this.SaveData(contract.Address, data, typeName);
				this.integration.logger.LogTrace("Saved contract data: " + data.Length + " bytes for " + contract.Address);
			}

			public ContractRegistry LoadContractRegistry()
			{
				var data = this.LoadData(this.integration.contractRegistryAddress);
				this.integration.logger.LogTrace("Received registry data: " + data.Length + " bytes");

				if (data.Length == 0)
				{
					return new ContractRegistry();
				}

				return StatefulObject.Create<ContractRegistry>(
					StateSerialization.DeserializeState(data));
			}

			public void SaveContractRegistry(ContractRegistry registry)
			{
				var data = StateSerialization.SerializeState(registry.GetState());
				var typeName = Encoding.ASCII.GetBytes(registry.GetType().Name);

				this.SaveData(this.integration.contractRegistryAddress, data, typeName);
				this.integration.logger.LogTrace("Saved registry data: " + data.Length + " bytes");
			}

			private ActionOrContract ReadFromStream()
			{
				this.requestStream.MoveNext().Wait();
				return this.requestStream.Current;
			}

			private void WriteToStream(ContractRequest request)
			{
				this.responseStream.WriteAsync(request).Wait();
			}

			private byte[] LoadData(Address address)
			{
				this.WriteToStream(new ContractRequest
				{
					Address = ByteString.CopyFrom(address.Value),
				});

				this.integration.logger.LogTrace("Sent request for data");

				var response = this.ReadFromStream();

				if (response.Contract == null)
				{
					throw new InvalidOperationException("Expected a Contract response");
				}

				if (address != new Address(response.Contract.Address.ToByteArray()))
				{
					throw new InvalidOperationException("Expected a Contract response for " + address + ", not for " + new Address(response.Contract.Address.ToByteArray()));
				}

				return response.Contract.Data.ToByteArray();
			}

			private void SaveData(Address address, byte[] data, byte[] typeName)
			{
				this.WriteToStream(new ContractRequest
				{
					Address = ByteString.CopyFrom(address.Value),
					Data = ByteString.CopyFrom(data),
					TypeName = ByteString.CopyFrom(typeName)
				});
			}
		}
	}
}