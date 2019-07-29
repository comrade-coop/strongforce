using System.Runtime.Serialization;

namespace StrongForce.Core
{
	public class Action
	{
		public Action(Address origin, Address target)
		{
			this.Sender = origin;
			this.Origin = origin;
			this.Target = target;
		}

		[IgnoreDataMember]
		public Address Origin { get; }

		[IgnoreDataMember]
		public Address Sender { get; }

		public Address Target { get; }
	}
}