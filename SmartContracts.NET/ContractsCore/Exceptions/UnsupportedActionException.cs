using System;
using Action = ContractsCore.Actions.Action;

namespace ContractsCore.Exceptions
{
	public class UnsupportedActionException : ArgumentException
	{
		public UnsupportedActionException(Address contract, Action action)
			: base(
				$"Contract with address \"{contract.Value}\" " +
				$"received an unsupported action: {action.GetType()}")
		{
		}
	}
}