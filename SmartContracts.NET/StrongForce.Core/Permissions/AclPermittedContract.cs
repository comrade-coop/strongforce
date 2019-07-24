using System;
using System.Collections.Generic;
using System.Linq;
using StrongForce.Core.Exceptions;
using StrongForce.Core.Permissions.Actions;

namespace StrongForce.Core.Permissions
{
	public abstract class AclPermittedContract : PermittedContract
	{
		protected AclPermittedContract(Address address, Address permissionManager)
			: this(address, permissionManager, new AccessControlList(null))
		{
		}

		protected AclPermittedContract(Address address, Address permissionManager, AccessControlList acl)
			: base(address)
		{
			this.Acl = acl;
			this.ConfigurePermissionManager(permissionManager);
		}

		protected AccessControlList Acl { get; }

		protected override bool HandleAction(Action action)
		{
			switch (action)
			{
				case RemovePermittedAddressAction permissionAction:
					this.HandleUpdatePermissionAction(permissionAction);
					return true;

				case TracingBulletAction bulletAction:
					this.ReceiveTracingBullet(bulletAction);
					return true;

				case ForwardAction forwardAction:
					this.HandleForwardAction(forwardAction);
					return true;

				default:
					return base.HandleAction(action);
			}
		}

		protected virtual void HandleForwardAction(ForwardAction action)
		{
			if (action.Target == null)
			{
				throw new ArgumentNullException(nameof(action.Target));
			}

			Action forwarded = action.ForwardedAction;

			if (action.ForwardingPath.Count == 1)
			{
				forwarded.Sender = this.Address;
				this.SendAction(forwarded);
			}
			else
			{
				Address nextAddress = action.ForwardingPath.Pop();
				action.Target = nextAddress;
				this.SendAction(action);
			}
		}

		protected override void ReceiveTracingBullet(TracingBulletAction action)
		{
			if (action.TracingAction.Origin == null)
			{
				action.TracingAction.Origin = action.Origin;
				action.TracingAction.Sender = action.Sender;
			}

			var permission = new Permission(action.TracingAction.GetType());

			if (Equals(action.TracingAction.Target, this.Address))
			{
				this.FindBulletPaths(action);
			}
			else if (this.Acl.HasPermission(action.TracingAction.Origin, permission, this.Address))
			{
				// Path Found
				var current = new TracingElement(this.Address, action.Predecessors);
				TracingElement a = action.SearchAddresses.FirstOrDefault(x => x.Equals(current));
				if (a == null)
				{
					return;
				}

				a.IsPath = true;
				a.Path.Push(this.Address);
			}
			else
			{
				this.GetAllowedForForwarding(action, action.SearchAddresses);
			}
		}

		protected virtual void FindBulletPaths(TracingBulletAction action)
		{
			var bfsAddresses = new List<TracingElement>();

			this.GetAllowedForForwarding(action, bfsAddresses);
			for (int i = 0; i < bfsAddresses.Count; i++)
			{
				TracingElement couple = bfsAddresses.Skip(i).First();

				Stack<Address> predecessors = couple.Path ?? new Stack<Address>(new[] {this.Address});
				var newAction = new TracingBulletAction(
					couple.Address,
					action.TracingAction,
					null,
					predecessors,
					bfsAddresses);
				this.SendAction(newAction);
			}

			List<Stack<Address>> addressQuery = bfsAddresses.Where(x => x.IsPath).Select(x => x.Path).ToList();
			action.CallBack(addressQuery, action.TracingAction);
		}

		protected virtual List<TracingElement> GetAllowedForForwarding(
			TracingBulletAction action,
			List<TracingElement> searchAddresses)
		{
			var permission = new Permission(action.TracingAction.GetType());
			foreach (Address address in this.Acl.GetPermittedAddresses(permission, this.Address))
			{
				var predecessors = new Stack<Address>();
				if (action.Predecessors != null)
				{
					predecessors = new Stack<Address>(action.Predecessors.Reverse());
				}

				if (!predecessors.Contains(this.Address))
				{
					searchAddresses.Add(new TracingElement(address, predecessors, false));
				}

				predecessors.Push(this.Address);
			}

			return searchAddresses;
		}

		protected override bool CheckPermission(Action action)
		{
			var permission = new Permission(action.GetType());
			if (!this.Acl.HasPermission(action.Sender, permission, this.Address))
			{
				throw new NoPermissionException(this, action.Sender, permission);
			}

			return true;
		}

		protected override void BulletTaken(List<Stack<Address>> paths, Action targetAction)
		{
			throw new NotImplementedException();
		}

		protected override void HandleAddPermissionAction(AddPermissionAction action)
		{
			this.Acl.AddPermission(action.PermittedAddress, action.Permission, action.Receiver);
		}

		protected override void HandleRemovePermissionAction(RemovePermissionAction action)
		{
			this.Acl.RemovePermission(action.Permission, action.Receiver);
		}

		protected void HandleUpdatePermissionAction(RemovePermittedAddressAction action)
		{
			this.Acl.RemovePermittedAddress(action.PermittedAddress, action.Permission, action.Receiver);
		}

		private void ConfigurePermissionManager(Address permissionManager)
		{
			this.Acl.AddPermission(
				permissionManager,
				new Permission(typeof(AddPermissionAction)),
				this.Address);
			this.Acl.AddPermission(
				permissionManager,
				new Permission(typeof(RemovePermissionAction)),
				this.Address);
			this.Acl.AddPermission(
				permissionManager,
				new Permission(typeof(RemovePermittedAddressAction)),
				this.Address);
		}
	}
}