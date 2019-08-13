using System;
using System.Collections.Generic;
using System.Linq;
using StrongForce.Core;
using StrongForce.Core.Permissions;

namespace StrongForce.Core.Tests.Mocks
{
	public class CreatorContract : Contract
	{
		public CreatorContract()
		{
		}

		public CreatorContract(Address initialAdmin)
			: base(initialAdmin)
		{
			this.Acl.AddPermission(
				initialAdmin,
				CreateContractAction.Type,
				this.Address);
		}

		public Address LastCreatedAddress { get; set; } = null;

		protected override bool HandlePayloadAction(PayloadAction action)
		{
			switch (action.Type)
			{
				case CreateContractAction.Type:
					this.HandleCreateContractAction(action.Payload);
					return true;

				default:
					return base.HandlePayloadAction(action);
			}
		}

		private void HandleCreateContractAction(IDictionary<string, object> payload)
		{
			var type = Type.GetType(payload.GetOrNull<string>(CreateContractAction.ContractType));
			this.LastCreatedAddress = this.CreateContract(type, new object[] { this.Address });
		}
	}
}