using System;

namespace StrongForce.Core.Exceptions
{
	public class UnsupportedActionException : ArgumentException
	{
		public UnsupportedActionException(Address contract, Action action)
			: base(
				$"Contract with address \"{contract.Value}\" " + $"received an unsupported action: {action.GetType()}")
		{
			this.Contract = contract;
			this.Action = action;
		}

		public Action Action { get; }

		public Address Contract { get; }
	}
}