using System.Collections.Generic;

namespace StrongForce.Core.Permissions
{
	public class RemovePermissionAction : Action
	{
		public RemovePermissionAction(
			Address target,
			Permission permission,
			Address receiver)
			: base(target)
		{
			this.Receiver = receiver;
			this.Permission = permission;
		}

		public Address Receiver { get; }

		public Permission Permission { get; }
	}
}