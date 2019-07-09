using System;
using System.Collections.Generic;
using ContractsCore;
using ContractsCore.Actions;
using ContractsCore.Contracts;

namespace Tendermint.Tests.Mocks
{
	public class DummyAction : ContractsCore.Actions.Action
	{
		public DummyAction(string hash, Address target, ContractsCore.Actions.Action forwarded = null, object objectProperty = null)
			: base(hash, target)
		{
			this.NextAction = forwarded;
			this.ObjectProperty = objectProperty;
		}

		public ContractsCore.Actions.Action NextAction { get; private set; }

		public object ObjectProperty { get; private set; }
	}
}