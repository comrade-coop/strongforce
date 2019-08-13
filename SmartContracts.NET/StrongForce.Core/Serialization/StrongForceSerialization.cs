using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace StrongForce.Core.Serialization
{
	public static class StrongForceSerialization
	{
		public static JsonSerializerSettings ContractSerializationSettings { get; } = new JsonSerializerSettings()
		{
			TypeNameHandling = TypeNameHandling.Auto,
		};

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

		public static string SerializeContract(Contract contract)
		{
			return JsonConvert.SerializeObject(contract, typeof(Contract), ContractSerializationSettings);
		}

		public static Contract DeserializeContract(string serialized)
		{
			return JsonConvert.DeserializeObject<Contract>(serialized, ContractSerializationSettings);
		}
	}
}