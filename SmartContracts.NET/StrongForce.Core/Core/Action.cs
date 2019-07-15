using System.Runtime.Serialization;

namespace StrongForce.Core
{
	public class Action
	{
		public Action(string hash, Address target)
		{
			this.Hash = hash;
			this.Target = target;
		}

		public string Hash { get; }

		[IgnoreDataMember]
		public Address Origin { get; internal set; }

		[IgnoreDataMember]
		public Address Sender { get; internal set; }

		public Address Target { get; }
	}
}