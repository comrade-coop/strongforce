using ContractsCore.Permissions;

namespace ContractsCore.Actions
{
	public class AddPermissionAction : Action
	{
		public AddPermissionAction(
			string hash,
			Address target,
			Permission permission,
			object permittedAddress,
			object nextAddress = null)
			: base(hash, target)
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