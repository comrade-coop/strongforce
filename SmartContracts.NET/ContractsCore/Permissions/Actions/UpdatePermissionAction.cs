
namespace StrongForce.Core.Permissions
{
	public class UpdatePermissionAction : Action
	{
		public UpdatePermissionAction(
			string hash,
			Address target,
			WildCard oldPermittedAddress,
			WildCard oldReceiver,
			Permission permission,
			WildCard newPermitedAddress,
			WildCard newReceiver)
			: base(hash, target)
		{
			this.OldPermittedAddress = oldPermittedAddress;
			this.NewPermittedAddress = newPermitedAddress;
			this.OldReceiver = oldReceiver;
			this.NewReceiver = newReceiver;
			this.Permission = permission;
		}

		public WildCard OldPermittedAddress { get; }

		public WildCard NewPermittedAddress { get; }

		public WildCard OldReceiver { get; }

		public WildCard NewReceiver { get; }

		public Permission Permission { get; }
	}
}