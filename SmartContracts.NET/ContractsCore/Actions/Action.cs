namespace ContractsCore.Actions
{
	public class Action
	{
		public Action(Address target)
		{
			this.Target = target;
		}

		public Address Origin { get; internal set; }

		public Address Sender { get; internal set; }

		public Address Target { get; }
	}
}