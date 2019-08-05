using System.Collections.Generic;
using System.Linq;
using StrongForce.Core;
using StrongForce.Core.Permissions;

namespace StrongForce.Core.Tests.Mocks
{
	public class CreatorContract : Contract
	{
		public CreatorContract(Address address)
			: base(address)
		{
		}

		public CreatorContract(Address address, Address initialAdmin)
			: base(address, initialAdmin)
		{
			this.Acl.AddPermission(
				initialAdmin,
				typeof(CreateContractAction),
				this.Address);
		}

		public Address LastCreatedAddress { get; private set; } = null;

		protected override bool HandleAction(ActionContext context, Action action)
		{
			switch (action)
			{
				case CreateContractAction createContractAction:
					this.HandleCreateContractAction(createContractAction);
					return true;

				default:
					return base.HandleAction(context, action);
			}
		}

		private void HandleCreateContractAction(CreateContractAction createContractAction)
		{
			this.LastCreatedAddress = this.CreateContract(createContractAction.ContractType, this.Address);
		}
	}
}