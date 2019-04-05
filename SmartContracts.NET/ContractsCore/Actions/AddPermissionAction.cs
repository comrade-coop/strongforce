using ContractsCore.Permissions;

namespace ContractsCore.Actions
{
	public class AddPermissionAction : Action
	{
		public AddPermissionAction(
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