using System;
using System.Collections.Generic;

namespace StrongForce.Core.Kits
{
	public abstract class Kit
	{
		public Func<Type, IDictionary<string, object>, Address> CreateContractHandler { get; set; }

		public System.Action<Address[], string, IDictionary<string, object>> SendActionHandler { get; set; }

		public abstract Address Instantiate(Address initialManager);

		protected void SendAction(Address[] targets, string type, IDictionary<string, object> payload)
		{
			this.SendActionHandler.Invoke(targets, type, payload);
		}

		protected Address CreateContract(Type contractType, IDictionary<string, object> payload)
		{
			return this.CreateContractHandler.Invoke(contractType, payload);
		}

		protected Address CreateContract<T>(IDictionary<string, object> payload)
		{
			return this.CreateContract(typeof(T), payload);
		}

		protected Address InstantiateSubKit(Kit subkit, Address initialManager)
		{
			subkit.CreateContractHandler = this.CreateContract;
			subkit.SendActionHandler = this.SendAction;

			return subkit.Instantiate(initialManager);
		}
	}
}