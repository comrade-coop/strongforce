using System;
using System.Collections.Generic;
using System.Linq;
using ContractsCore;
using ContractsCore.Actions;
using ContractsCore.Contracts;

namespace Tendermint
{
	public class RemoteContractRegistry : ContractRegistry
	{
		private readonly ISet<Address> contractsUsed;
		private readonly Func<Address, Contract> contractGetter;

		public RemoteContractRegistry(
			IAddressFactory addressFactory,
			object initialState,
			Func<Address, Contract> contractGetter)
			: base(addressFactory, initialState)
		{
			this.contractsUsed = new HashSet<Address>();
			this.contractGetter = contractGetter;
		}

		public RemoteContractRegistry(IAddressFactory addressFactory, Func<Address, Contract> contractGetter)
			: this(addressFactory, new object(), contractGetter)
		{
		}

		public void SendAction(Address target, ContractsCore.Actions.Action action)
		{
			this.HandleAction(action, target);
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