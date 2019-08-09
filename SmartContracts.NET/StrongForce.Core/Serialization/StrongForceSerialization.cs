using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace StrongForce.Core.Serialization
{
	public static class StrongForceSerialization
	{
		public static JsonSerializerSettings ActionSerializationSettings { get; } = new JsonSerializerSettings()
		{
			SerializationBinder = new FilteredSerializationBinder()
			{
				WhitelistedBaseTypes = new HashSet<Type> { typeof(Action), typeof(Address) },
				BlacklistedTypes = new HashSet<Type> { },
			},
			TypeNameHandling = TypeNameHandling.Auto,
		};

		public static JsonSerializerSettings ContractSerializationSettings { get; } = new JsonSerializerSettings()
		{
			TypeNameHandling = TypeNameHandling.Auto,
		};

		public static string SerializeAction(Action action)
		{
			return JsonConvert.SerializeObject(action, typeof(Action), ActionSerializationSettings);
		}

		public static Action DeserializeAction(string serialized)
		{
			return JsonConvert.DeserializeObject<Action>(serialized, ActionSerializationSettings);
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