using System.Collections.Generic;

namespace StrongForce.Core.Permissions
{
	public class RemovePermissionAction : Action
	{
		public RemovePermissionAction(
			Address target,
			Permission permission,
			HashSet<Address> permittedAddress)
			: base(target)
		{
			this.PermittedAddress = permittedAddress;
			this.Permission = permission;
		}

		public HashSet<Address> PermittedAddress { get; }

		public Permission Permission { get; }
	}
}