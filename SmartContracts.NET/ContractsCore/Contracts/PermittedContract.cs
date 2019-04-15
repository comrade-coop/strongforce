using System;
using ContractsCore.Actions;
using ContractsCore.Exceptions;
using ContractsCore.Permissions;
using Action = ContractsCore.Actions.Action;

namespace ContractsCore.Contracts
{
	public abstract class PermittedContract : Contract
	{
		protected PermittedContract(Address address)
			: base(address)
		{
		}

		public abstract bool HasPermission(Address address, Permission permission);

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
			if (!this.HasPermission(action.Sender, permission))
			{
				throw new NoPermissionException(this, action.Sender, permission);
			}
		}

		protected abstract void HandleAddPermissionAction(AddPermissionAction action);

		protected abstract void HandleRemovePermissionAction(RemovePermissionAction action);
	}
}