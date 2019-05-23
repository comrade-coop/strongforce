using ContractsCore.Permissions;

namespace ContractsCore.Actions
{
	public class RemovePermissionAction : Action
	{
		public RemovePermissionAction(
			Address target,
			Permission permission,
			object permittedAddress,
			object nextAddress = null)
			: base(target)
		{
			this.PermittedAddress = permittedAddress;
			this.NextAddress = nextAddress;
			if (nextAddress == null)
			{
				this.NextAddress = target;
			}

			this.Permission = permission;
		}

		public object PermittedAddress { get; }

		public object NextAddress { get; }

		public Permission Permission { get; }
	}
}