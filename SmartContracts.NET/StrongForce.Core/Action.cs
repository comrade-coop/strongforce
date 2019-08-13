using System;

namespace StrongForce.Core
{
	public abstract class Action
	{
		public Action(Address target, Address origin, Address sender)
		{
			this.Target = target;
			this.Origin = origin;
			this.Sender = sender;
		}

		public Address Target { get; }

		public Address Origin { get; }

		public Address Sender { get; }

		public abstract string Type { get; }

		public abstract Address FinalTarget { get; }
	}
}