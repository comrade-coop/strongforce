using System;

namespace ContractsCore.Exceptions
{
	public class NoPermissionException : Exception
	{
		public NoPermissionException(Contract contract, Address actionSender, Type actionType)
			: base($"Address \"{actionSender}\" doesn\'t have permission \"{actionType.Name}\" on {contract}")
		{
		}
	}
}