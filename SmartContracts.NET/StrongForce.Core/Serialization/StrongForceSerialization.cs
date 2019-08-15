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
				{ "Targets", targets.Select(x => x.AsString()) },
				{ "Type", type },
				{ "Payload", payload },
			};
			return StateSerialization.SerializeState(action);
		}

		public static Tuple<Address[], string, IDictionary<string, object>> DeserializeAction(byte[] serialized)
		{
			var action = StateSerialization.DeserializeState(serialized);
			var targets = action
				.GetOrNull<IList<object>>("Targets")
				.Select(x => (x as string).AsAddress())
				.ToArray();
			var type = action.GetOrNull<string>("Type");
			var payload = action.GetOrNull<IDictionary<string, object>>("Payload");
			return Tuple.Create(targets, type, payload);
		}

		public static byte[] SerializeContract(Contract contract)
		{
			var dictionary = new Dictionary<string, object>()
			{
				{ "Type", contract.GetType().AssemblyQualifiedName },
				{ "State", contract.GetState() },
			};
			return StateSerialization.SerializeState(dictionary);
		}

		public static Contract DeserializeContract(byte[] serialized)
		{
			var dictionary = StateSerialization.DeserializeState(serialized);

			var type = Type.GetType(dictionary.GetOrNull<string>("Type"));
			var state = dictionary.GetOrNull<IDictionary<string, object>>("State");

			var contract = Activator.CreateInstance(type) as Contract;

			contract.SetState(state);

			return contract;
		}
	}
}