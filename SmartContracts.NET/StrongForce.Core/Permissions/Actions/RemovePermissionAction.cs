namespace StrongForce.Core.Permissions
{
	public class RemovePermissionAction : Action
	{
		public RemovePermissionAction(
			string hash,
			Address target,
			Permission permission,
			WildCard permittedAddress,
			WildCard receiver)
			: base(hash, target)
		{
			this.PermittedAddress = permittedAddress;
			this.Receiver = receiver;
			this.Permission = permission;
		}

		public WildCard PermittedAddress { get; }

		public WildCard Receiver { get; }

		public Permission Permission { get; }
	}
}