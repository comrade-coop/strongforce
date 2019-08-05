using System;
using System.Collections.Generic;
using System.Linq;
using StrongForce.Core;
using Action = StrongForce.Core.Action;

namespace Tendermint
{
	public class RemoteContractRegistry : ContractRegistry
	{
		private readonly ISet<Address> contractsUsed = new HashSet<Address>();
		private readonly Func<Address, Contract> contractGetter;

		public RemoteContractRegistry(Func<Address, Contract> contractGetter)
			: base()
		{
			this.contractGetter = contractGetter;
		}

		public IEnumerable<Contract> GetUsedContracts()
		{
			return this.contractsUsed.Select(x => base.GetContract(x));
		}

		public override Contract GetContract(Address address)
		{
			this.contractsUsed.Add(address);
			var cachedContract = base.GetContract(address);
			if (cachedContract == null)
			{
				this.RegisterContract(this.contractGetter.Invoke(address));
				return base.GetContract(address);
			}
			else
			{
				return cachedContract;
			}
		}

		protected override void SetContract(Contract contract)
		{
			this.contractsUsed.Add(contract.Address);
			base.SetContract(contract);
		}
	}
}