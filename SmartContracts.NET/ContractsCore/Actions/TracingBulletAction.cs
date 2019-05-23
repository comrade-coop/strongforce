using System;
using System.Collections.Generic;
using ContractsCore.Contracts;

namespace ContractsCore.Actions
{
	public class TracingBulletAction : Action
	{
		public TracingBulletAction(
			Address target,
			Action action,
			Action<List<Stack<Address>>, Action> returnWais,
			Stack<Address> predecessors,
			ref List<TracingElement> bfsAddresses)
			: base(target)
		{
			this.TracingAction = action;
			this.CallBack = returnWais;
			this.Predecessors = predecessors;
			this.BfsAddresses = bfsAddresses;
		}

		public Action TracingAction;

		public List<TracingElement> BfsAddresses;

		public Stack<Address> Predecessors = new Stack<Address>();

		public Action<List<Stack<Address>>, Action> CallBack;
	}
}