using System.Collections.Generic;

namespace ContractsCore.Actions
{
	public class ForwardAction : Action
	{
		public ForwardAction(Address target, Action forwarded, Stack<Address> way)
			: base(target)
		{
			this.ForwardedAction = forwarded;
			this.WayForForwarding = way;
		}

		public Action ForwardedAction { get; }

		public Stack<Address> WayForForwarding { get; }
	}
}