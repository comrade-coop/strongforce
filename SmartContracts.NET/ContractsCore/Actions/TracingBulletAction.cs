using ContractsCore.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContractsCore.Actions
{
	public class TracingBulletAction : Actions.Action
	{
		public Action TracingAction;

		public List<TracingElement> BfsAddresses;

		public Stack<Address> Predecessors = new Stack<Address>();

		public Action<List<Stack<Address>>, Action> CallBack;

		public TracingBulletAction(
			string hash,
			Address target,
			Action action,
			Action<List<Stack<Address>>, Action> returnWais,
			Stack<Address> predecessors,
			ref List<TracingElement> bfsAddresses)
			: base(hash, target)
		{
			this.TracingAction = action;
			this.CallBack = returnWais;
			this.Predecessors = predecessors;
			this.BfsAddresses = bfsAddresses;
		}
	}
}