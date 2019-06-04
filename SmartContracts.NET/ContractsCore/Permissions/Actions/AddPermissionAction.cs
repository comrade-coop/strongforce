using System;

namespace StrongForce.Core.Permissions
{
	public class AddPermissionAction : Action
	{
		public AddPermissionAction(
			string hash,
			Address target,
			Permission permission,
			WildCard permittedAddress,
			WildCard receiver = null)
			: base(hash, target)
		{
			this.PermittedAddress = permittedAddress;
			this.Receiver = receiver ?? new AddressWildCard() { target };
			this.Permission = permission;
		}

		public AddPermissionAction(
			string hash,
			Address target,
			Permission permission,
			Address permittedAddress,
			Address receiver = null)
			: base(hash, target)
		{
			this.PermittedAddress = new AddressWildCard() { permittedAddress };
			this.Receiver = receiver != null ? new AddressWildCard() { receiver } : new AddressWildCard() { target };
			this.Permission = permission;
		}

		public WildCard PermittedAddress { get; }

		public WildCard Receiver { get; }

		public Permission Permission { get; }

		public static implicit operator AddPermissionAction(UpdatePermissionAction v)
		{
			throw new NotImplementedException();
		}
	}
}