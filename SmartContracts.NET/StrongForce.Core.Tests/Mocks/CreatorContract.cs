using System;
using System.Collections.Generic;
using System.Linq;
using StrongForce.Core;
using StrongForce.Core.Extensions;
using StrongForce.Core.Permissions;

namespace StrongForce.Core.Tests.Mocks
{
	public class CreatorContract : Contract
	{
		public Address LastCreatedAddress { get; set; } = null;

		public override IDictionary<string, object> GetState()
		{
			var state = base.GetState();

			state.Add("LastCreatedAddress", this.LastCreatedAddress.AsString());

			return state;
		}

		public override void SetState(IDictionary<string, object> state)
		{
			this.LastCreatedAddress = state.GetOrNull<string>("LastCreatedAddress").AsAddress();
			base.SetState(state);
		}

		protected override void Initialize(IDictionary<string, object> payload)
		{
			base.Initialize(payload);

			var admin = payload.GetOrNull<string>("Admin").AsAddress();

			this.Acl.AddPermission(
				admin,
				CreateContractAction.Type,
				this.Address);
		}

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
			this.LastCreatedAddress = this.CreateContract(type, new Dictionary<string, object>() { { "Admin", this.Address.AsString() } });
		}
	}
}