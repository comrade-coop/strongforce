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

		public AccessControlList Acl { get; private set; } = new AccessControlList();

		public override string ToString()
		{
			return $"{this.Address} ({this.GetType()})";
		}

		internal bool Receive(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			if (!this.CheckPermission(action))
			{
				throw new NoPermissionException(this, action.Sender, new Permission(action.GetType()));
			}

			return this.HandleAction(action);
		}

		protected bool CheckPermission(Action action)
		{
			var permission = new Permission(action.GetType());
			return this.Acl.HasPermission(action.Sender, permission, this.Address);
		}

		protected virtual bool HandleAction(Action action)
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

				/*
				case ForwardAction forwardAction:
					this.HandleForwardAction(forwardAction);
					return true;
				*/

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