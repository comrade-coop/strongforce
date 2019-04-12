using System;
using ContractsCore.Actions;
using ContractsCore.Exceptions;
using ContractsCore.Permissions;
using Action = ContractsCore.Actions.Action;

namespace ContractsCore.Contracts
{
	public abstract class PermittedContract : Contract
	{
		private readonly AccessControlList acl;

		protected PermittedContract(Address address, Address permissionManager)
			: this(address, permissionManager, new AccessControlList())
		{
		}

		protected PermittedContract(Address address, Address permissionManager, AccessControlList acl)
			: base(address)
		{
			this.acl = acl;
			this.ConfigurePermissionManager(permissionManager);
		}

		public bool HasPermission(Address address, Permission permission)
		{
			return this.acl.HasPermission(address, permission);
		}

		protected internal override bool Receive(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			switch (action)
			{
				case AddPermissionAction permissionAction:
					this.HandleAddPermissionAction(permissionAction);
					return true;

				case RemovePermissionAction permissionAction:
					this.HandleRemovePermissionAction(permissionAction);
					return true;

				default:
					return this.HandleReceivedAction(action);
			}
		}

		protected void RequirePermission(Action action)
		{
			var permission = new Permission(action.GetType());
			if (!this.acl.HasPermission(action.Sender, permission))
			{
				throw new NoPermissionException(this, action.Sender, permission);
			}
		}

		private void ConfigurePermissionManager(Address permissionManager)
		{
			this.acl.AddPermission(permissionManager, new Permission(typeof(AddPermissionAction)));
			this.acl.AddPermission(permissionManager, new Permission(typeof(RemovePermissionAction)));
		}

		private void HandleAddPermissionAction(AddPermissionAction action)
		{
			this.RequirePermission(action);
			this.acl.AddPermission(action.PermittedAddress, action.Permission);
		}

		private void HandleRemovePermissionAction(RemovePermissionAction action)
		{
			this.RequirePermission(action);
			this.acl.RemovePermission(action.PermittedAddress, action.Permission);
		}
	}
}