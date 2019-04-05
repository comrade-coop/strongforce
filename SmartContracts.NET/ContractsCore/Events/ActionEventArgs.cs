using System;
using Action = ContractsCore.Actions.Action;

namespace ContractsCore.Events
{
	public class ActionEventArgs : EventArgs
	{
		public ActionEventArgs(Action action)
		{
			this.Action = action;
		}

		public Action Action { get; }
	}
}