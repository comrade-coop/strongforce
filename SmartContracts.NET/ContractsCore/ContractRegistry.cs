using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using ContractsCore.Actions;
using ContractsCore.Contracts;
using ContractsCore.Events;
using ContractsCore.Exceptions;
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

			contract.Send += (_, actionArgs) => this.HandleSendActionEvent(contract.Address, actionArgs);

			contract.Forward += (_, actionArgs) => this.HandleForwardActionEvent(contract.Address, actionArgs);
		}

		public Contract GetContractForAddress(Address address)
		{
			return addressesToContracts[address];
		}

		protected bool HandleAction(Action action, Address targetAddress)
		{
			if (targetAddress == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return this.addressesToContracts.ContainsKey(targetAddress)
				&& this.addressesToContracts[targetAddress].Receive(action);
		}

		private void HandleSendActionEvent(object sender, ActionEventArgs actionArgs)
		{
			Action action = actionArgs.Action;
			if (action == null || action.Target == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (action.Origin != null)
			{
				throw new UnknownActionOringExeption(action);
			}

			action.Origin = sender as Address;
			action.Sender = sender as Address;
			this.HandleAction(actionArgs.Action, action.Target);
		}

		private void HandleForwardActionEvent(object sender, ActionEventArgs actionArgs)
		{
			if (!(actionArgs.Action is ForwardAction action) || action.ForwardedAction == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (action.ForwardedAction.Origin == null)
			{
				throw new UnknownActionOringExeption(action);
			}

			if (action.Origin == null)
			{
				action.Origin = sender as Address;
			}

			action.Sender = sender as Address;
			action.ForwardedAction.Sender = sender as Address;
			this.HandleAction(action, action.Target);
		}
	}
}