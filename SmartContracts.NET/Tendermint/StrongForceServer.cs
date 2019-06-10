using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using ContractsCore;
using ContractsCore.Actions;
using ContractsCore.Contracts;
using Google.Protobuf;
using Grpc.Core;
using Strongforce;

namespace Tendermint
{
	public class StrongForceServer : StrongForce.StrongForceBase
	{
		public override async Task ExecuteAction(IAsyncStreamReader<ActionOrContract> requestStream, IServerStreamWriter<ContractRequest> responseStream, ServerCallContext context)
		{
			BinaryFormatter actionFormatter = new BinaryFormatter()
			{
				Binder = new FilteredSerializationBinder()
				{
					WhitelistedBaseTypes = new HashSet<Type>() { typeof(ContractsCore.Actions.Action) },
					BlacklistedTypes = new HashSet<Type>() { typeof(ContractsCore.Actions.TracingBulletAction) },
				},
			};

			BinaryFormatter contractFormatter = new BinaryFormatter(); // No filter here, trusted

			var pendingTasks = new Dictionary<Address, TaskCompletionSource<Contract>>();

			var registry = new RemoteContractRegistry((address) =>
			{
				pendingTasks[address] = new TaskCompletionSource<Contract>();
				responseStream.WriteAsync(new ContractRequest
				{
					Address = ByteString.CopyFrom(address.Value),
				}).Wait();
				return pendingTasks[address].Task.Result;
			});

			while (await requestStream.MoveNext())
			{
				var actionOrContract = requestStream.Current;
				if (actionOrContract.Action != null)
				{
					var address = new Address(actionOrContract.Action.Address.ToByteArray());

					var memoryStream = new MemoryStream(actionOrContract.Action.Data.ToByteArray());
					var action = actionFormatter.Deserialize(memoryStream) as ContractsCore.Actions.Action;

					#pragma warning disable CS4014 // Awaiting will deadlock
					Task.Run(() => registry.SendAction(address, action));
					#pragma warning restore CS4014
				}
				else if (actionOrContract.Contract != null)
				{
					var address = new Address(actionOrContract.Contract.Address.ToByteArray());

					var memoryStream = new MemoryStream(actionOrContract.Contract.Data.ToByteArray());
					var contract = contractFormatter.Deserialize(memoryStream) as Contract;

					pendingTasks[address].SetResult(contract);
				}
			}

			foreach (var contract in registry.GetUsedContracts())
			{
				var memoryStream = new MemoryStream();
				actionFormatter.Serialize(memoryStream, contract);
				var data = memoryStream.ToArray();

				await responseStream.WriteAsync(new ContractRequest
				{
					Address = ByteString.CopyFrom(contract.Address.Value),
					Data = ByteString.CopyFrom(data),
				});
			}
		}
	}
}