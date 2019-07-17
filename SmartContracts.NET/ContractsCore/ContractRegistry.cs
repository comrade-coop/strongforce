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
		private static ContractRegistry instance;

		private readonly IDictionary<Address, Contract> addressesToContracts;
		private readonly IAddressFactory addressFactory;

		protected ContractRegistry(IAddressFactory addressFactory, object initialState)
		{
			this.addressesToContracts = new SortedDictionary<Address, Contract>();
			this.addressFactory = addressFactory;
		}

		protected ContractRegistry(IAddressFactory addressFactory)
			: this(addressFactory, new object())
		{
		}

		public static ContractRegistry Instance
		{
			get
			{
				if (instance == null)
				{
					throw new Exception("Contract Registry has not been instantiated with its dependencies.");
				}

				return instance;
			}
		}

		public static void Initialise(IAddressFactory addressFactory, object initialState = null)
		{
			instance = new ContractRegistry(addressFactory, initialState);
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

		public void RegisterContract(Contract contract)
		{
			if (contract == null)
			{
				throw new ArgumentNullException(nameof(contract));
			}

			Address address = contract.Address;
			SetContract(contract);

			contract.Send += (_, actionArgs) => this.HandleSendActionEvent(contract.Address, actionArgs);
			contract.Forward += (_, actionArgs) => this.HandleForwardActionEvent(contract.Address, actionArgs);
		}

		protected bool HandleAction(Action action, Address targetAddress)
		{
			if (targetAddress == null)
			{
				throw new ArgumentNullException(nameof(action));
			}


			Contract contract = this.GetContract(targetAddress);
			return contract?.Receive(action) ?? false;
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