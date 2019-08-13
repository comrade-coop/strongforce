using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using StrongForce.Core.Exceptions;
using StrongForce.Core.Permissions;

namespace StrongForce.Core.Kits
{
	public abstract class Kit
	{
		[IgnoreDataMember]
		public Func<Type, object[], Address> CreateContractHandler { get; set; }

		[IgnoreDataMember]
		public System.Action<Address[], string, IDictionary<string, object>> SendActionHandler { get; set; }

		public abstract Address Instantiate(Address initialManager);

		protected void SendAction(Address[] targets, string type, IDictionary<string, object> payload)
		{
			this.SendActionHandler.Invoke(targets, type, payload);
		}

		protected Address CreateContract(Type contractType, params object[] constructorParameters)
		{
			return this.CreateContractHandler.Invoke(contractType, constructorParameters);
		}

		protected Address CreateContract<T>(params object[] constructorParameters)
		{
			return this.CreateContract(typeof(T), constructorParameters);
		}

		protected Address InstantiateSubKit(Kit subkit, Address initialManager)
		{
			subkit.CreateContractHandler = this.CreateContract;
			subkit.SendActionHandler = this.SendAction;

			return subkit.Instantiate(initialManager);
		}
	}
}