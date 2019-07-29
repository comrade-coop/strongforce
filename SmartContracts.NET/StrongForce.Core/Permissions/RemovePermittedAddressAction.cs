using System.Collections.Generic;

namespace StrongForce.Core.Permissions
{
	public class RemovePermittedAddressAction : Action
	{
		public RemovePermittedAddressAction(
			Address origin,
			Address target,
			Permission permission,
			Address permittedAddress,
			Address receiver)
			: base(origin, target)
		{
			this.PermittedAddress = permittedAddress;
			this.Receiver = receiver;
			this.Permission = permission;
		}

		public Address PermittedAddress { get; }

		public Address Receiver { get; }

		public Permission Permission { get; }
	}
}