using System;
using System.Runtime.Serialization;

namespace StrongForce.Core
{
	public class ActionContext
	{
		public ActionContext(Address origin, Address sender)
		{
			this.Origin = origin;
			this.Sender = sender;
		}

		public ActionContext(Address sender)
			: this(sender, sender)
		{
		}

		public Address Origin { get; }

		public Address Sender { get; }
	}
}