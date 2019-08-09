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
				typeof(DummyAction),
				this.Address);
		}

		protected override bool HandleAction(ActionContext context, Action action)
		{
			switch (action)
			{
				case DummyAction dummy:
					if (dummy.NextAction != null)
					{
						this.SendAction(dummy.NextAction);
					}

					return true;

				default:
					return base.HandleAction(context, action);
			}
		}
	}
}