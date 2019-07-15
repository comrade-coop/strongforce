using System;
using System.Collections.Generic;
using System.Text;

namespace StrongForce.Core.Permissions
{
	public class TracingBulletAction : Action
	{
		public List<TracingElement> BfsAddresses;

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

		public Action TracingAction { get; set; }

		public Stack<Address> Predecessors { get; set; }

		public Action<List<Stack<Address>>, Action> CallBack { get; set; }
	}
}