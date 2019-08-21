using System;
using System.Collections.Generic;
using StrongForce.Core;
using StrongForce.Core.Extensions;
using StrongForce.Core.Permissions;

namespace StrongForce.Integrations.Cosmos.Tests.Mocks
{
	public class DummyContract : Contract
	{

		protected override void Initialize(IDictionary<string, object> payload)
		{
			base.Initialize(payload);

			if (payload.ContainsKey("Admin"))
			{
				this.Acl.AddPermission(
					payload.GetAddress("Admin"),
					DummyAction.Type,
					this.Address);
			}
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