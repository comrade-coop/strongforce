using System;
using System.Collections.Generic;

namespace StrongForce.Core
{
	public interface IIntegrationFacade
	{
		event Action<Address, Address[], string, IDictionary<string, object>> ReceiveMessage;

		event Action DropCaches;

		(BaseContract, Action<Message>) LoadContract(Address address, ContractHandlers handlers);

		ContractRegistryState LoadRegistryState();

		void SaveContract(BaseContract contract);

		void SaveRegistryState(ContractRegistryState state);
	}
}