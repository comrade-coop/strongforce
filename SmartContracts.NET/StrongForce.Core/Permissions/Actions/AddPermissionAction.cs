using System;

namespace StrongForce.Core.Permissions
{
	public class AddPermissionAction : Action
	{
		public AddPermissionAction(
			Address target,
			Permission permission,
			IWildCard permittedAddress,
			IWildCard receiver = null)
			: base(target)
		{
			this.PermittedAddress = permittedAddress;
			this.Receiver = receiver ?? new AddressWildCard() { target };
			this.Permission = permission;
		}

		public AddPermissionAction(
			Address target,
			Permission permission,
			Address permittedAddress,
			Address receiver = null)
			: base(target)
		{
			this.PermittedAddress = new AddressWildCard() { permittedAddress };
			this.Receiver = receiver != null ? new AddressWildCard() { receiver } : new AddressWildCard() { target };
			this.Permission = permission;
		}

		public IWildCard PermittedAddress { get; }

		public IWildCard Receiver { get; }

		public Permission Permission { get; }

		public static implicit operator AddPermissionAction(UpdatePermissionAction v)
		{
			throw new NotImplementedException();
		}
	}
}