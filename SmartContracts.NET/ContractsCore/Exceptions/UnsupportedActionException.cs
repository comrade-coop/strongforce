using System;

namespace StrongForce.Core.Exceptions
{
	public class UnsupportedActionException : ArgumentException
	{
		public Action Action { get; private set; }

		public Address Contract { get; private set; }

		public UnsupportedActionException(Address contract, Action action)
			: base(
				$"Contract with address \"{contract.Value}\" " + $"received an unsupported action: {action.GetType()}")
		{
			this.Contract = contract;
			this.Action = action;
		}
	}
}