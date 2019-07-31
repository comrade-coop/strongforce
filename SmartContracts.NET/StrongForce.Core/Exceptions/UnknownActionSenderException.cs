using System;

namespace StrongForce.Core.Exceptions
{
	public class UnknownActionSenderException : Exception
	{
		public UnknownActionSenderException(Action action, Address expectedFrom)
			: base(
			$"Action \"{action.GetType()}-{action.GetHashCode()}\" has inapproperiate sender {action.Sender}, expected {expectedFrom} ")
		{
			this.Action = action;
			this.ExpectedSender = expectedFrom;
		}

		public Action Action { get; }

		public Address ExpectedSender { get; }
	}
}