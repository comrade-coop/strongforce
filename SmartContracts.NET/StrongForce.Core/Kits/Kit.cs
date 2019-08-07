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
		public System.Action<Action> SendActionHandler { get; set; }

		public abstract Address Instantiate(Address initialManager);

		protected void SendAction(Action action)
		{
			this.SendActionHandler.Invoke(action);
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