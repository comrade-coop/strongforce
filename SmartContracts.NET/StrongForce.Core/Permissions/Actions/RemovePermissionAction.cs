namespace StrongForce.Core.Permissions
{
	public class RemovePermissionAction : Action
	{
		public RemovePermissionAction(
			Address target,
			Permission permission,
			IWildCard permittedAddress,
			IWildCard receiver)
			: base(target)
		{
			this.PermittedAddress = permittedAddress;
			this.Receiver = receiver;
			this.Permission = permission;
		}

		public IWildCard PermittedAddress { get; }

		public IWildCard Receiver { get; }

		public Permission Permission { get; }
	}
}