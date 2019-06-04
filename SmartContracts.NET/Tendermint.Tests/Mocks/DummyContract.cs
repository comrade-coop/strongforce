using System;
using StrongForce.Core;
using Action = StrongForce.Core.Action;

namespace Tendermint.Tests.Mocks
{
	public class DummyContract : Contract
	{
		public DummyContract(Address address)
			: base(address)
		{
		}

		protected override object GetState() => new object { };

		protected override bool HandleReceivedAction(Action action)
		{
			switch (action)
			{
				case DummyAction dummy:
					if (dummy.NextAction != null) this.SendEvent(dummy.NextAction);
					return true;

				default:
					return false;
			}
		}
	}
}