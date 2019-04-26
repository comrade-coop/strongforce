using System;
using ContractsCore.Contracts;
using ContractsCore.Permissions;

namespace ContractsCore.Exceptions
{
	public class NoPermissionException : Exception
	{
		public readonly Permission Permission;

		public readonly Contract Contract;

		public readonly Address ActionSender;

		public NoPermissionException(Contract contract, Address actionSender, Permission permission)
			: base($"Address \"{actionSender.ToBase64String()}\" does not have permission \"{permission}\" on {contract}")
		{
			this.Permission = permission;
			this.Contract = contract;
			this.ActionSender = actionSender;
		}
	}
}