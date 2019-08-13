using System;
using Newtonsoft.Json;
using StrongForce.Core;
using StrongForce.Core.Permissions;
using Action = StrongForce.Core.Action;

namespace StrongForce.Integrations.Cosmos.Tests.Mocks
{
	public class DummyContract : Contract
	{
		public DummyContract()
		{
		}

		public DummyContract(Address initialAdmin)
			: base(initialAdmin)
		{
			this.Acl.AddPermission(
				initialAdmin,
				DummyAction.Type,
				null);
		}

		protected override bool HandlePayloadAction(PayloadAction action)
		{
			switch (action.Type)
			{
				case DummyAction.Type:
					return true;

				default:
					return base.HandlePayloadAction(action);
			}
		}
	}
}