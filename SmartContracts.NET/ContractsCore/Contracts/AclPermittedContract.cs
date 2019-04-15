using ContractsCore.Actions;
using ContractsCore.Permissions;

namespace ContractsCore.Contracts
{
	public abstract class AclPermittedContract : PermittedContract
	{
		private readonly AccessControlList acl;

		protected AclPermittedContract(Address address, Address permissionManager)
			: this(address, permissionManager, new AccessControlList())
		{
		}

		protected AclPermittedContract(Address address, Address permissionManager, AccessControlList acl)
			: base(address)
		{
			this.acl = acl;
			this.ConfigurePermissionManager(permissionManager);
		}

		public override bool HasPermission(Address address, Permission permission)
		{
			return this.acl.HasPermission(address, permission);
		}

		protected override void HandleAddPermissionAction(AddPermissionAction action)
		{
			this.RequirePermission(action);
			this.acl.AddPermission(action.PermittedAddress, action.Permission);
		}

		protected override void HandleRemovePermissionAction(RemovePermissionAction action)
		{
			this.RequirePermission(action);
			this.acl.RemovePermission(action.PermittedAddress, action.Permission);
		}

		private void ConfigurePermissionManager(Address permissionManager)
		{
			this.acl.AddPermission(permissionManager, new Permission(typeof(AddPermissionAction)));
			this.acl.AddPermission(permissionManager, new Permission(typeof(RemovePermissionAction)));
		}
	}
}