using System;
using Action = StrongForce.Core.Action;

namespace StrongForce.Core
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