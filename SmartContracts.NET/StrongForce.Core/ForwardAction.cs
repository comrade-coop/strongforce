using System.Collections.Generic;

namespace StrongForce.Core
{
	public class ForwardAction : Action
	{
		public ForwardAction(Address target, Action forwarded, Stack<Address> forwardingPath)
			: base(target)
		{
			this.ForwardedAction = forwarded;
			this.ForwardingPath = forwardingPath;
		}

		public Action ForwardedAction { get; }

		public Stack<Address> ForwardingPath { get; }
	}
}