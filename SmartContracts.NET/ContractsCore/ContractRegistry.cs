using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ContractsCore.Contracts;
using ContractsCore.Events;
using Action = ContractsCore.Actions.Action;

namespace ContractsCore
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

		public Contract GetContract(Address address)
		{
			return this.addressesToContracts[address];
		}

		public object GetState()
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
			if (this.addressesToContracts.ContainsKey(address))
			{
				throw new ArgumentException(
					$"Contract with same address: {address.ToBase64String()} has already been registered");
			}

			this.addressesToContracts[address] = contract;
			contract.Send += this.HandleSendActionEvent;
			contract.Forward += this.HandleForwardActionEvent;
		}

		public bool HandleAction(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			Address targetAddress = action.Target;
			return this.addressesToContracts.ContainsKey(targetAddress)
				&& this.addressesToContracts[targetAddress].Receive(action);
		}

		private void HandleSendActionEvent(object sender, ActionEventArgs actionArgs)
		{
			this.HandleAction(actionArgs.Action);
		}

		private void HandleForwardActionEvent(object sender, ActionEventArgs actionArgs)
		{
			this.HandleAction(actionArgs.Action);
		}
	}
}