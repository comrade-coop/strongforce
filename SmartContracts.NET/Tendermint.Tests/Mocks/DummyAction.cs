using System;
using System.Collections.Generic;
using ContractsCore;
using ContractsCore.Actions;
using ContractsCore.Contracts;

namespace Tendermint.Tests.Mocks
{
	public class DummyAction : ContractsCore.Actions.Action
	{
		public ContractsCore.Actions.Action NextAction { get; }

		public DummyAction(string hash, Address target, ContractsCore.Actions.Action forwarded = null) : base(hash, target)
		{
			this.NextAction = forwarded;
		}
	}
}