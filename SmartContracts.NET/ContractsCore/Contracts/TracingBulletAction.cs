using System;
using System.Collections.Generic;
using System.Text;

namespace ContractsCore.Contracts
{
	public class TracingBulletAction: Actions.Action
	{
		public Actions.Action TracingAction;

		public List<TracingElement> BfsAddresses;

		public Stack<Address> Predecessors = new Stack<Address>();

		public Action<List<Stack<Address>>, Actions.Action> CallBack;

		public TracingBulletAction(
			string hash,
			Address target,
			Actions.Action action,
			Action<List<Stack<Address>>, Actions.Action> returnWais,
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