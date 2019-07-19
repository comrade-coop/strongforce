using System;
using System.Collections.Generic;
using StrongForce.Core.Exceptions;

namespace StrongForce.Core
{
	public class ContractRegistry
	{
		private readonly IDictionary<Address, Contract> addressesToContracts;

		public ContractRegistry(object initialState)
		{
			this.addressesToContracts = new SortedDictionary<Address, Contract>();
		}

		public ContractRegistry()
			: this(new object())
		{
		}

		public virtual Contract GetContract(Address address)
		{
			if (this.addressesToContracts.TryGetValue(address, out var contract))
			{
				return contract;
			}
			else
			{
				return null;
			}
		}

		public virtual object GetState()
		{
			throw new NotImplementedException();
		}

		public void RegisterContract(Contract contract)
		{
			if (contract == null)
			{
				throw new ArgumentNullException(nameof(contract));
			}

			Address address = contract.Address;
			this.SetContract(contract);

			contract.Registry = this;
		}

		protected virtual void SetContract(Contract contract)
		{
			Address address = contract.Address;

			if (this.addressesToContracts.ContainsKey(address))
			{
				throw new ArgumentException(
					$"Contract with same address: {address.ToBase64String()} has already been registered");
			}

			this.addressesToContracts[address] = contract;
		}

		public bool HandleAction(Action action)
		{
			if (action.Target == null || action.Sender == null || action.Origin == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			var contract = this.GetContract(action.Target);
			return contract != null ? contract.Receive(action) : false;
		}
	}
}