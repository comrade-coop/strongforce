using System;
using System.Collections.Generic;

namespace StrongForce.Core
{
	public class InMemoryIntegrationFacade : IIntegrationFacade
	{
		private Dictionary<Address, (Type, IDictionary<string, object>)> storedContracts = new Dictionary<Address, (Type, IDictionary<string, object>)>();

		public event Action<Address, Address[], string, IDictionary<string, object>> ReceiveMessage;

		public event Action DropCaches;

		public (BaseContract, Action<Message>) LoadContract(Address address, ContractHandlers handlers)
		{
			var (type, state) = this.storedContracts[address];
			return BaseContract.Create(type, address, state, handlers, true);
		}

		public void SaveContract(BaseContract contract)
		{
			var state = contract.GetState();
			var type = contract.GetType();
			this.storedContracts[contract.Address] = (type, state);
		}

		public void CreateContract(Type type, Address address, IDictionary<string, object> payload = null)
		{
			var (contract, receiver) = BaseContract.Create(type, address, payload, default(ContractHandlers));

			this.SaveContract(contract);

			this.DropCaches?.Invoke();
		}

		public void SendMessage(Address from, Address[] targets, string type, IDictionary<string, object> payload)
		{
			try
			{
				this.ReceiveMessage?.Invoke(from, targets, type, payload);
			}
			catch(Exception)
			{
				this.DropCaches?.Invoke();
				throw;
			}
		}
	}
}