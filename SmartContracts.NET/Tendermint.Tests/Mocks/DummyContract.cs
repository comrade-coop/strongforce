using System;
using System.Collections.Generic;
using ContractsCore;
using ContractsCore.Actions;
using ContractsCore.Contracts;

namespace Tendermint.Tests.Mocks
{
	public class DummyContract : Contract
	{
		public DummyContract(Address address)
			: base(address)
		{
		}

		protected override object GetState() => new object { };

		protected override bool HandleReceivedAction(ContractsCore.Actions.Action action)
		{
			switch (action)
			{
				case DummyAction dummy:
					if (dummy.NextAction != null) this.OnSend(dummy.NextAction);
					return true;

				default:
					return false;
			}
		}
	}
}