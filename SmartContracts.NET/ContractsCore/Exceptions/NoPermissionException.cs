using System;
using ContractsCore.Contracts;
using ContractsCore.Permissions;

namespace ContractsCore.Exceptions
{
	public class NoPermissionException : Exception
	{
		public NoPermissionException(Contract contract, Address actionSender, Permission permission)
			: base($"Address \"{actionSender.ToBase64String()}\" does not have permission \"{permission}\" on {contract}")
		{
		}
	}
}