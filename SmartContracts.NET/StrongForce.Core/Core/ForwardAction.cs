using System;
using System.Collections.Generic;
using System.Text;

namespace StrongForce.Core
{
	public class ForwardAction : Action
	{
		public ForwardAction(string hash, Address target, Action forwarded, Stack<Address> way)
			: base(hash, target)
		{
			this.ForwardedAction = forwarded;
			this.WayForForwarding = way;
		}

		public Action ForwardedAction { get; set; }

		public Stack<Address> WayForForwarding { get; set; }
	}
}