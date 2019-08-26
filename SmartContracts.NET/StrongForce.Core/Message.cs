using System;
using System.Collections.Generic;

namespace StrongForce.Core
{
	public class Message
	{
		public Message(Address target, Address origin, Address sender, string type, IDictionary<string, object> payload)
		{
			this.Target = target;
			this.Origin = origin;
			this.Sender = sender;
			this.Type = type;
			this.Payload = payload;
		}

		public Address Target { get; }

		public Address Origin { get; }

		public Address Sender { get; }

		public string Type { get; }

		public IDictionary<string, object> Payload { get; }

		public virtual Address FinalTarget { get => this.Target; }
	}
}