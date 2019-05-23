using System;
using System.Collections.Generic;
using ContractsCore.Actions;
using ContractsCore.Contracts;
using ContractsCore.Events;
using ContractsCore.Exceptions;
using Action = ContractsCore.Actions.Action;

namespace ContractsCore
{
	public class ContractRegistry : IContractRegistry
	{
		private static ContractRegistry instance;

		private readonly IDictionary<Address, Contract> addressesToContracts;
		private readonly IAddressFactory addressFactory;

		public ContractRegistry(IAddressFactory addressFactory, object initialState)
		{
			this.addressesToContracts = new SortedDictionary<Address, Contract>();
			this.addressFactory = addressFactory;
		}

		public ContractRegistry(IAddressFactory addressFactory)
			: this(addressFactory, new object())
		{
		}

		public static void InitialiseInstance(IAddressFactory addressFactory, object initialState = null)
		{
			instance = new ContractRegistry(addressFactory, initialState);
		}

		public static IContractRegistry GetInstance()
		{
			if (instance == null)
			{
				throw new Exception("Contract Registry has not been instantiated with its dependencies.");
			}

			return instance;
		}

		public T GetContract<T>(Address address)
			where T : Contract
		{
			try
			{
				return (T) this.addressesToContracts[address];
			}
			catch (KeyNotFoundException e)
			{
				throw new UnknownContractException(address);
			}
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

			Address contractAddress = contract.Address;
			if (contractAddress != null && this.addressesToContracts.ContainsKey(contractAddress))
			{
				throw new ArgumentException(
					$"Contract with same address: {contractAddress.ToBase64String()} has already been registered");
			}

			Address address = this.addressFactory.Create();
			contract.Address = address;

			this.addressesToContracts[address] = contract;

			contract.Send += (_, actionArgs) => this.HandleSendActionEvent(contract.Address, actionArgs);

			contract.Forward += (_, actionArgs) => this.HandleForwardActionEvent(contract.Address, actionArgs);
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