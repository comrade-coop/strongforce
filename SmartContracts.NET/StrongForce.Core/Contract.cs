using System;
using System.Collections.Generic;
using StrongForce.Core.Exceptions;
using StrongForce.Core.Permissions;

namespace StrongForce.Core
{
	public abstract class Contract
	{
		protected Contract(Address address)
		{
			this.Address = address;
		}

		protected Contract(Address address, Address initialAdmin)
			: this(address)
		{
			this.Acl.AddPermission(
				initialAdmin,
				new Permission(typeof(AddPermissionAction)),
				this.Address);
			this.Acl.AddPermission(
				initialAdmin,
				new Permission(typeof(RemovePermissionAction)),
				this.Address);
			this.Acl.AddPermission(
				initialAdmin,
				new Permission(typeof(RemovePermittedAddressAction)),
				this.Address);
		}

		public delegate void ActionEventHandler(Action action);

		internal event ActionEventHandler SendActionEvent;

		public Address Address { get; }

		public AccessControlList Acl { get; } = new AccessControlList();

		public override string ToString()
		{
			return $"{this.Address} ({this.GetType()})";
		}

		internal bool Receive(ActionContext context, Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (!this.CheckPermission(context, action))
			{
				throw new NoPermissionException(this, context.Sender, new Permission(action.GetType()));
			}

			return this.HandleAction(context, action);
		}

		protected bool CheckPermission(ActionContext context, Action action)
		{
			var checkedAction = action;

			if (action is ForwardAction forwardAction)
			{
				checkedAction = forwardAction.FinalAction;
			}

			var permission = new Permission(checkedAction.GetType());
			return this.Acl.HasPermission(context.Sender, permission, checkedAction.Target);
		}

		protected virtual bool HandleAction(ActionContext context, Action action)
		{
			switch (action)
			{
				case AddPermissionAction permissionAction:
					this.Acl.AddPermission(
						permissionAction.PermittedAddress,
						permissionAction.Permission,
						permissionAction.Receiver);
					return true;

				case RemovePermissionAction permissionAction:
					this.Acl.RemovePermission(
						permissionAction.Permission,
						permissionAction.Receiver);
					return true;

				case RemovePermittedAddressAction permissionAction:
					this.Acl.RemovePermittedAddress(
						permissionAction.PermittedAddress,
						permissionAction.Permission,
						permissionAction.Receiver);
					return true;

				case ForwardAction forwardAction:
					this.SendAction(forwardAction.NextAction);
					return true;

				default:
					return false;
			}
		}

		protected void SendAction(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException();
			}

			this.SendActionEvent?.Invoke(action);
		}
	}
}