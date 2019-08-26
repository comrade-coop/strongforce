using System;
using System.Collections.Generic;
using System.Linq;

namespace StrongForce.Core
{
	public class ForwardMessage : Message
	{
		public ForwardMessage(Address target, Address origin, Address sender, string type, IDictionary<string, object> payload, Address[] nextTargets, ulong forwardId)
			: base(target, origin, sender, type, payload)
		{
			this.NextTargets = nextTargets;
			this.ForwardId = forwardId;
		}

		public IList<Address> NextTargets { get; }

		public ulong ForwardId { get; }

		public override Address FinalTarget { get => this.NextTargets.Last(); }
	}
}