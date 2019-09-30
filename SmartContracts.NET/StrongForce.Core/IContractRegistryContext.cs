using System;
using System.Collections.Generic;

namespace StrongForce.Core
{
	public interface IContractRegistryContext
	{
		BaseContract LoadContract(Address address);

		void SaveContract(BaseContract contract);
	}
}