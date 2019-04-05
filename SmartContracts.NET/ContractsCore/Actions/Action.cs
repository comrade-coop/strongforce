namespace ContractsCore.Actions
{
	public class Action
	{
		public Action(string hash, Address target)
		{
			this.Hash = hash;
			this.Target = target;
		}

		public string Hash { get; }

		public Address Origin { get; internal set; }

		public Address Sender { get; internal set; }

		public Address Target { get; }
	}
}