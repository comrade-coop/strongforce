using System;
using StrongForce.Core;
using StrongForce.Core.Permissions;
using Action = StrongForce.Core.Action;

namespace Tendermint.Tests.Mocks
{
	public class DummyContract : Contract
	{
		public DummyContract(Address address)
			: base(address)
		{
		}

		public DummyContract(Address address, Address initialAdmin)
			: base(address, initialAdmin)
		{
			this.Acl.AddPermission(
				initialAdmin,
				new Permission(typeof(DummyAction)),
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