using System;
using System.Collections.Generic;

namespace StrongForce.Core.Permissions
{
	public abstract class PermittedContract : Contract
	{
		protected PermittedContract(Address address)
			: base(address)
		{
		}

		internal override bool Receive(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			this.CheckPermission(action);
			this.HandleReceivedAction(action);

			return true;
		}

		protected override bool HandleReceivedAction(Action action)
		{
			switch (action)
			{
				case AddPermissionAction permissionAction:
					this.HandleAddPermissionAction(permissionAction);
					return true;

				case RemovePermissionAction permissionAction:
					this.HandleRemovePermissionAction(permissionAction);
					return true;

				default:
					return false;
			}
		}

		protected abstract void ReceiveTracingBullet(TracingBulletAction action);

		protected abstract void BulletTaken(List<Stack<Address>> ways, Action targetAction);

		protected abstract bool CheckPermission(Action action);

		protected abstract void HandleAddPermissionAction(AddPermissionAction action);

		protected abstract void HandleRemovePermissionAction(RemovePermissionAction action);
	}
}