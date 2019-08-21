using System;
using System.Collections.Generic;

namespace StrongForce.Core.Kits
{
	public abstract class Kit
	{
		public System.Action<Type, Address, IDictionary<string, object>> CreateContractHandler { get; set; }

		public Func<Address> CreateAddressHandler { get; set; }

		public abstract Address Instantiate(Address initialManager);

		protected Address CreateAddress()
		{
			return this.CreateAddressHandler.Invoke();
		}

		protected void CreateContract(Type contractType, Address address, IDictionary<string, object> payload)
		{
			this.CreateContractHandler.Invoke(contractType, address, payload);
		}

		protected Address CreateContract(Type contractType, IDictionary<string, object> payload)
		{
			var address = this.CreateAddress();

			this.CreateContract(contractType, address, payload);

			return address;
		}

		protected void CreateContract<T>(Address address, IDictionary<string, object> payload)
		{
			this.CreateContract(typeof(T), address, payload);
		}

		protected Address CreateContract<T>(IDictionary<string, object> payload)
		{
			return this.CreateContract(typeof(T), payload);
		}

		protected Address InstantiateSubKit(Kit subkit, Address initialManager)
		{
			subkit.CreateContractHandler = this.CreateContractHandler;
			subkit.CreateAddressHandler = this.CreateAddressHandler;

			return subkit.Instantiate(initialManager);
		}
	}
}