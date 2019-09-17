using System;
using System.Collections.Generic;
using StrongForce.Core.Extensions;

namespace StrongForce.Core
{
	/// <summary>
	/// Used to generate address values, which are to be injected in all entities in the StrongForce instance.
	/// </summary>
	public abstract class BaseAddressFactory : IStatefulObject
	{
		public static BaseAddressFactory Create(Type type, IDictionary<string, object> state)
		{
			if (!typeof(BaseAddressFactory).IsAssignableFrom(type))
			{
				throw new ArgumentOutOfRangeException(nameof(type));
			}

			var factory = (BaseAddressFactory)Activator.CreateInstance(type);

			if (state != null)
			{
				factory.SetState(factory.GetState().MergeStateWith(state));
			}

			return factory;
		}

		public abstract Address CreateAddress();

		public virtual IDictionary<string, object> GetState()
		{
			return new Dictionary<string, object>();
		}

		protected virtual void SetState(IDictionary<string, object> state)
		{
			// pass
		}
	}
}