namespace StrongForce.Core.Permissions
{
	public class UpdatePermissionAction : Action
	{
		public UpdatePermissionAction(
			Address target,
			IWildCard oldPermittedAddress,
			IWildCard oldReceiver,
			Permission permission,
			IWildCard newPermitedAddress,
			IWildCard newReceiver)
			: base(target)
		{
			this.OldPermittedAddress = oldPermittedAddress;
			this.NewPermittedAddress = newPermitedAddress;
			this.OldReceiver = oldReceiver;
			this.NewReceiver = newReceiver;
			this.Permission = permission;
		}

		public IWildCard OldPermittedAddress { get; }

		public IWildCard NewPermittedAddress { get; }

		public IWildCard OldReceiver { get; }

		public IWildCard NewReceiver { get; }

		public Permission Permission { get; }
	}
}