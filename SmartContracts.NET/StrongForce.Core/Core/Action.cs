using System.Runtime.Serialization;

namespace StrongForce.Core
{
	public class Action
	{
		public Action(Address target)
		{
			this.Target = target;
		}

		[IgnoreDataMember]
		public Address Origin { get; internal set; }

		[IgnoreDataMember]
		public Address Sender { get; internal set; }

		public Address Target { get; }
	}
}