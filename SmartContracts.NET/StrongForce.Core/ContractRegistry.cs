using System;
using System.Collections.Generic;
using StrongForce.Core.Exceptions;

namespace StrongForce.Core
{
	public class ContractRegistry
	{
		private IDictionary<Address, Contract> addressesToContracts = new SortedDictionary<Address, Contract>();

		// TODO: This Set uses reference equality and hashcode, but it should be made more sophisticated
		// Also, it might be better to implement it as Dictionary<Action, int>, in case an action is sent twice
		private ISet<Action> forwardedActions = new HashSet<Action>();

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

		public bool HandleAction(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return this.HandleAction(action.Sender, action);
		}

		public bool HandleAction(Address from, Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (action.Target == null)
			{
				throw new ArgumentNullException(nameof(action.Target));
			}

			if (!action.IsConfigured)
			{
				throw new ArgumentOutOfRangeException(nameof(action));
			}

			if (!action.Sender.Equals(from))
			{
				throw new UnknownActionSenderException(action, from);
			}

			if (!action.Sender.Equals(action.Origin))
			{
				if (this.forwardedActions.Contains(action))
				{
					this.forwardedActions.Remove(action);
				}
				else
				{
					throw new UnknownActionOriginException(action, action.Sender);
				}
			}
			else if (action is ForwardAction forwardAction)
			{
				if (forwardAction.Origin.Equals(forwardAction.NextAction.Origin))
				{
					var checkedAction = forwardAction.NextAction;
					while (checkedAction is ForwardAction checkedForwardAction)
					{
						if (!checkedAction.IsConfigured)
						{
							throw new ArgumentOutOfRangeException(nameof(action));
						}

						checkedAction = checkedForwardAction.NextAction;
					}

					this.forwardedActions.Add(forwardAction.NextAction);
				}
				else
				{
					throw new UnknownActionOriginException(forwardAction.NextAction, forwardAction.Origin);
				}
			}

			Contract contract = this.GetContract(action.Target);
			return contract != null && contract.Receive(action);
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