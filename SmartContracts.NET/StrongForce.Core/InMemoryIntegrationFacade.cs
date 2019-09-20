using System;
using System.Collections.Generic;

namespace StrongForce.Core
{
	public class InMemoryIntegrationFacade : IIntegrationFacade
	{
		private Dictionary<Address, (Type, IDictionary<string, object>)> storedContracts = new Dictionary<Address, (Type, IDictionary<string, object>)>();

		private IDictionary<string, object> storedRegistryState = new Dictionary<string, object>();

		public event Action<Address, Address[], string, IDictionary<string, object>> ReceiveMessage;

		public event Action DropCaches;

		public BaseContract LoadContract(Address address)
		{
			var (type, state) = this.storedContracts[address];
			return (BaseContract)StatefulObject.Create(type, state, true);
		}

		public void SaveContract(BaseContract contract)
		{
			var state = contract.GetState();
			var type = contract.GetType();
			this.storedContracts[contract.Address] = (type, state);
		}

		public ContractRegistryState LoadRegistryState()
		{
			return StatefulObject.Create<ContractRegistryState>(this.storedRegistryState);
		}

		public void SaveRegistryState(ContractRegistryState state)
		{
			this.storedRegistryState = state.GetState();
		}

		public void CreateContract(Type type, IContractContext context, IDictionary<string, object> payload = null)
		{
			var contract = (BaseContract)StatefulObject.Create(type, payload);
			contract.RegisterWithRegistry(context);

			this.SaveContract(contract);

			this.DropCaches?.Invoke();
		}

		public void SendMessage(Address from, Address[] targets, string type, IDictionary<string, object> payload)
		{
			try
			{
				this.ReceiveMessage?.Invoke(from, targets, type, payload);
			}
			catch (Exception)
			{
				this.DropCaches?.Invoke();
				throw;
			}
		}

		public void DropCache()
		{
			this.DropCaches?.Invoke();
		}
	}
}