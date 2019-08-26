using System;
using System.Collections.Generic;
using System.Linq;
using StrongForce.Core.Extensions;

namespace StrongForce.Core.Serialization
{
	public static class StrongForceSerialization
	{
		public static byte[] SerializeAction(Address[] targets, string type, IDictionary<string, object> payload)
		{
			var action = new Dictionary<string, object>()
			{
				{ "Targets", targets.Select(x => x?.ToString()) },
				{ "Type", type },
				{ "Payload", payload },
			};
			return StateSerialization.SerializeState(action);
		}

		public static Tuple<Address[], string, IDictionary<string, object>> DeserializeAction(byte[] serialized)
		{
			var action = StateSerialization.DeserializeState(serialized);
			var targets = action
				.GetList<string>("Targets").Select(Address.Parse)
				.ToArray();
			var type = action.GetString("Type");
			var payload = action.GetDictionary("Payload");
			return Tuple.Create(targets, type, payload);
		}

		public static byte[] SerializeContract(BaseContract contract)
		{
			var dictionary = new Dictionary<string, object>()
			{
				{ "Type", contract.GetType().AssemblyQualifiedName },
				{ "State", contract.GetState() },
			};
			return StateSerialization.SerializeState(dictionary);
		}

		public static (BaseContract, Action<Message>) DeserializeContract(Address address, ContractHandlers handlers, byte[] serialized)
		{
			var dictionary = StateSerialization.DeserializeState(serialized);

			var type = Type.GetType(dictionary.GetString("Type"));
			var state = dictionary.GetDictionary("State");

			return BaseContract.Create(type, address, state, handlers, true);
		}
	}
}