using System;
using System.Collections.Generic;

namespace StrongForce.Core
{
	public class EventAction : Action
	{
		public EventAction(Address target, Address origin, Address sender, string type, IDictionary<string, object> payload)
			: base(target, origin, sender)
		{
			this.Type = type;
			this.Payload = payload;
		}

		public override string Type { get; }

		public IDictionary<string, object> Payload { get; }

		public override Address FinalTarget { get => this.Target; }
	}
}