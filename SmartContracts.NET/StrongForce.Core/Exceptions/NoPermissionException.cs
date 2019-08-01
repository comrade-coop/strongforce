using System;
using StrongForce.Core.Permissions;

namespace StrongForce.Core.Exceptions
{
	public class NoPermissionException : Exception
	{
		public NoPermissionException(Contract contract, Address actionSender, Permission permission)
			: base($"Address \"{actionSender.ToBase64String()}\" does not have permission \"{permission}\" on {contract}")
		{
			this.Permission = permission;
			this.Contract = contract;
			this.ActionSender = actionSender;
		}

		public Permission Permission { get; }

		public Contract Contract { get; }

		public Address ActionSender { get; }
	}
}