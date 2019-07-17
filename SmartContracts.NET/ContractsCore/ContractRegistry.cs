using System;
using System.Collections.Generic;
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
		private readonly IAddressFactory addressFactory;

		public ContractRegistry(object initialState)
		{
			this.addressesToContracts = new SortedDictionary<Address, Contract>();
			this.addressFactory = new RandomAddressFactory();
		}

		public ContractRegistry()
			: this(new object())
		{
		}

		public virtual Contract GetContract(Address address)
		{
			return this.addressesToContracts.TryGetValue(address, out Contract contract) ? contract : null;
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

		public virtual object GetState()
		{
			throw new NotImplementedException();
		}

		public Address RegisterContract(Contract contract)
		{
			if (contract == null)
			{
				throw new ArgumentNullException(nameof(contract));
			}

			contract.Address = this.addressFactory.Create();
			this.SetContract(contract);

			contract.Send += (_, actionArgs) => this.HandleSendActionEvent(contract.Address, actionArgs);
			contract.Forward += (_, actionArgs) => this.HandleForwardActionEvent(contract.Address, actionArgs);

			return contract.Address;
		}

		protected bool HandleAction(Action action)
		{
			if (action.GetType() == typeof(CreateContractAction))
			{
				var contractAction = (CreateContractAction) action;
				var contract = (Contract) Activator.CreateInstance(
					contractAction.ContractType, contractAction.ConstructorArgs);
				contract.Address = this.addressFactory.Create();
				this.addressesToContracts[contract.Address] = contract;
				contractAction.ContractAddress = contract.Address;

				return true;
			}
			else
			{
				if (action.Target == null)
				{
					throw new ArgumentNullException(nameof(action));
				}

				Contract contract = this.GetContract(action.Target);
				return contract?.Receive(action) ?? false;
			}
		}

		private void HandleSendActionEvent(object sender, ActionEventArgs actionArgs)
		{
			Action action = actionArgs.Action;
			if (action?.Target == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (action.Origin != null)
			{
				throw new UnknownActionOringExeption(action);
			}

			action.Origin = sender as Address;
			action.Sender = sender as Address;
			this.HandleAction(actionArgs.Action);
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
			this.HandleAction(action);
		}
	}
}