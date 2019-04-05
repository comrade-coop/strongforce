using ContractsCore.Permissions;

namespace ContractsCore.Actions
{
	public class RemovePermissionAction : Action
	{
		public RemovePermissionAction(
			string hash,
			Address origin,
			Address sender,
			Address target,
			Address permittedAddress,
			Permission permission)
			: base(hash, origin, sender, target)
		{
			this.PermittedAddress = permittedAddress;
			this.Permission = permission;
		}

		public Address PermittedAddress { get; }

		public Permission Permission { get; }
	}
}