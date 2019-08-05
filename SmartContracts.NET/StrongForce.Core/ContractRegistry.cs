using System;
using System.Collections.Generic;
using StrongForce.Core.Exceptions;

namespace StrongForce.Core
{
	public class ContractRegistry
	{
		private IDictionary<Address, Contract> addressesToContracts = new SortedDictionary<Address, Contract>();

		// TODO: This Dictionary uses reference equality and reference-based hashcode, which would break whenever actions are serialized / deserialized
		private IDictionary<Action, Address> forwardedActionsOrigins = new SortedDictionary<Action, Address>();

		public virtual Contract GetContract(Address address)
		{
			return this.addressesToContracts.TryGetValue(address, out Contract contract) ? contract : null;
		}

		public void RegisterContract(Contract contract)
		{
			if (contract == null)
			{
				throw new ArgumentNullException(nameof(contract));
			}

			Address address = contract.Address;

			this.SetContract(contract);

			contract.SendActionEvent += (action) => this.HandleAction(address, action);
		}

		public bool HandleAction(Address sender, Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (action.Target == null)
			{
				throw new ArgumentNullException(nameof(action.Target));
			}

			Address origin = sender;

			if (this.forwardedActionsOrigins.ContainsKey(action))
			{
				origin = this.forwardedActionsOrigins[action];
				this.forwardedActionsOrigins.Remove(action);
			}

			if (action is ForwardAction forwardAction)
			{
				this.forwardedActionsOrigins[forwardAction.NextAction] = origin;
			}

			var context = new ActionContext(origin, sender);
			var contract = this.GetContract(action.Target);

			return contract != null && contract.Receive(context, action);
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
	}
}