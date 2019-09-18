using System;
using System.Collections.Generic;
using StrongForce.Core.Extensions;

namespace StrongForce.Core
{
	public abstract class StatefulObject
	{
		public static StatefulObject Create(Type type, IDictionary<string, object> payload, bool deserializing = false)
		{
			if (!typeof(StatefulObject).IsAssignableFrom(type))
			{
				throw new ArgumentOutOfRangeException("Cannot create stateful object of type " + type);
			}

			var statefulObject = (StatefulObject)Activator.CreateInstance(type);

			if (!deserializing)
			{
				payload = statefulObject.GetState().MergeState(payload);
			}

			statefulObject.SetState(payload);

			return statefulObject;
		}

		public static T Create<T>(IDictionary<string, object> payload)
			where T : StatefulObject, new()
		{
			return (T)StatefulObject.Create(typeof(T), payload);
		}

		public static T CreateFromState<T>(IDictionary<string, object> payload)
			where T : StatefulObject, new()
		{
			return (T)StatefulObject.Create(typeof(T), payload, true);
		}

		public virtual IDictionary<string, object> GetState()
		{
			return new Dictionary<string, object>();
		}

		protected virtual void SetState(IDictionary<string, object> state)
		{
		}
	}
}