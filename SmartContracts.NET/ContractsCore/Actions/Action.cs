namespace ContractsCore.Actions
{
	public class Action
	{
		public Action(string hash, Address origin, Address sender, Address target)
		{
			this.Hash = hash;
			this.Origin = origin;
			this.Sender = sender;
			this.Target = target;
		}

		public string Hash { get; }

		public Address Origin { get; }

		public Address Sender { get; }

		public Address Target { get; }
	}
}