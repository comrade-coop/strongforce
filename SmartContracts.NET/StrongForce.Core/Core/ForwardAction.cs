using System;
using System.Collections.Generic;
using System.Text;

namespace StrongForce.Core
{
	public class ForwardAction : Action
	{
		public ForwardAction(Address target, Action forwarded, Stack<Address> way)
			: base(target)
		{
			this.ForwardedAction = forwarded;
			this.WayForForwarding = way;
		}

		public Action ForwardedAction { get; set; }

		public Stack<Address> WayForForwarding { get; set; }
	}
}