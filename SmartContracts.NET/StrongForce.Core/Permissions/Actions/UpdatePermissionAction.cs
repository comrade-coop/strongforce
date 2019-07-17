using System.Collections.Generic;

namespace StrongForce.Core.Permissions
{
	public class UpdatePermissionAction : Action
	{
		public UpdatePermissionAction(
			Address target,
			HashSet<Address> oldPermittedAddress,
			Permission permission,
			HashSet<Address> newPermitedAddress,
			HashSet<Address> newReceiver)
			: base(target)
		{
			this.OldPermittedAddress = oldPermittedAddress;
			this.NewPermittedAddress = newPermitedAddress;
			this.NewReceiver = newReceiver;
			this.Permission = permission;
		}

		public HashSet<Address> OldPermittedAddress { get; }

		public HashSet<Address> NewPermittedAddress { get; }

		public HashSet<Address> NewReceiver { get; }

		public Permission Permission { get; }
	}
}