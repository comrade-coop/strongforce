using System;
using System.Collections.Generic;
using System.Linq;
using StrongForce.Core.Exceptions;

namespace StrongForce.Core
{
	public class ContractRegistry
	{
		private IAddressFactory addressFactory;

		private IDictionary<Address, Contract> addressesToContracts = new SortedDictionary<Address, Contract>();

		// TODO: This Dictionary uses reference equality and reference-based hashcode, which would break whenever actions are serialized / deserialized
		private IDictionary<Action, Address> forwardedActionsOrigins = new SortedDictionary<Action, Address>();

		public ContractRegistry(IAddressFactory addressFactory)
		{
			this.addressFactory = addressFactory;
		}

		public ContractRegistry()
			: this(new RandomAddressFactory())
		{
		}

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
			contract.CreateContractEvent += this.CreateContract;
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

		public Address CreateContract(Type contractType, params object[] additionalParameters)
		{
			if (!typeof(Contract).IsAssignableFrom(contractType))
			{
				throw new ArgumentOutOfRangeException(nameof(contractType));
			}

			var newAddress = this.addressFactory.Create();

			var parameters = (new object[]
			{
				newAddress,
			}).Concat(additionalParameters).ToArray();

			var newContract = (Contract)Activator.CreateInstance(contractType, parameters);

			this.RegisterContract(newContract);

			return newAddress;
		}

		public Address CreateContract<T>(params object[] constructorParameters)
		{
			return this.CreateContract(typeof(T), constructorParameters);
		}

		protected virtual void SetContract(Contract contract)
		{
			Address address = contract.Address;

			if (this.addressesToContracts.ContainsKey(address))
			{
				throw new InvalidOperationException(
					$"Contract with same address: {address.ToBase64String()} has already been registered");
			}

			this.addressesToContracts[address] = contract;
		}
	}
}