using System;
using System.Collections.Generic;
using System.Text;

namespace StrongForce.Core
{
	public class ForwardAction : Action
	{
		public Action ForwardedAction;

		public Stack<Address> WayForForwarding;

		public ForwardAction(string hash, Address target, Action forwarded, Stack<Address> way) : base(hash, target)
		{
			this.ForwardedAction = forwarded;
			this.WayForForwarding = way;
		}
	}
}