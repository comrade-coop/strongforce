using System;
using StrongForce.Core.Permissions;

namespace StrongForce.Core.Exceptions
{
	public class NoPermissionException : Exception
	{
		public NoPermissionException(Address contract, Address origin, Permission neededPermission)
			: base($"Missing permission \"{neededPermission}\" on {contract} (on action originating from \"{origin}\")")
		{
			this.NeededPermission = neededPermission;
			this.Contract = contract;
			this.Origin = origin;
		}

		public Address Contract { get; }

		public Address Origin { get; }

		public Permission NeededPermission { get; }
	}
}