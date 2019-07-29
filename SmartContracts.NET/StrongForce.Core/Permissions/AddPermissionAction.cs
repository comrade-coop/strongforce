using System;
using System.Collections.Generic;

namespace StrongForce.Core.Permissions
{
	public class AddPermissionAction : Action
	{
		public AddPermissionAction(
			Address origin,
			Address target,
			Permission permission,
			Address permittedAddress,
			Address receiver = null)
			: base(origin, target)
		{
			this.PermittedAddress = permittedAddress;
			this.Receiver = receiver ?? target;
			this.Permission = permission;
		}

		public Address PermittedAddress { get; }

		public Address Receiver { get; }

		public Permission Permission { get; }
	}
}