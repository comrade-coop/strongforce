using System;
using System.Collections.Generic;

namespace StrongForce.Core.Permissions.Actions
{
	public class TracingBulletAction : Action
	{
		public TracingBulletAction(
			Address target,
			Action action,
			Action<List<Stack<Address>>, Action> returnPaths,
			Stack<Address> predecessors,
			List<TracingElement> bfsAddresses = null)
			: base(target)
		{
			this.TracingAction = action;
			this.CallBack = returnPaths;
			this.Predecessors = predecessors;
			this.SearchAddresses = bfsAddresses ?? new List<TracingElement>();
		}

		public List<TracingElement> SearchAddresses { get; }

		public Action TracingAction { get; }

		public Stack<Address> Predecessors { get; }

		public Action<List<Stack<Address>>, Action> CallBack { get; }
	}
}