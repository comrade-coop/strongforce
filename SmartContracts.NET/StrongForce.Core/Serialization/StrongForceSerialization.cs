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
			var targets = action.GetList<Address>("Targets").ToArray();
			var type = action.Get<string>("Type");
			var payload = action.Get<IDictionary<string, object>>("Payload");
			return Tuple.Create(targets, type, payload);
		}

		public static byte[] SerializeStatefulObject(StatefulObject statefulObject)
		{
			var dictionary = new Dictionary<string, object>()
			{
				{ "Type", statefulObject.GetType().AssemblyQualifiedName },
				{ "State", statefulObject.GetState() },
			};
			return StateSerialization.SerializeState(dictionary);
		}

		public static StatefulObject DeserializeStatefulObject(byte[] serialized)
		{
			var dictionary = StateSerialization.DeserializeState(serialized);

			var type = Type.GetType(dictionary.Get<string>("Type"));
			var state = dictionary.Get<IDictionary<string, object>>("State");

			return StatefulObject.Create(type, state, true);
		}
	}
}