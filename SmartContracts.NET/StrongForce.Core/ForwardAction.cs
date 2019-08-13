using System;
using System.Collections.Generic;
using System.Linq;

namespace StrongForce.Core
{
	public class ForwardAction : Action
	{
		public ForwardAction(Address target, Address[] nextTargets, Address origin, Address sender, ulong nextId, string type, IDictionary<string, object> payload)
			: base(target, origin, sender)
		{
			this.NextTargets = nextTargets;
			this.FinalTarget = nextTargets.Last();
			this.NextId = nextId;
			this.Type = type;
			this.Payload = payload;
		}

		public IList<Address> NextTargets { get; }

		public override Address FinalTarget { get; }

		public ulong NextId { get; }

		public override string Type { get; }

		public IDictionary<string, object> Payload { get; }
	}
}