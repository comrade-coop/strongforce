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

		public Permission Permission { get; private set; }

		public Contract Contract { get; private set; }

		public Address ActionSender { get; private set; }
	}
}