using System;
using System.Collections.Generic;
using System.Linq;
using ContractsCore.Actions;
using ContractsCore.Exceptions;
using ContractsCore.Permissions;
using Action = ContractsCore.Actions.Action;

namespace ContractsCore.Contracts
{
	public abstract class AclPermittedContract : PermittedContract
	{
		private readonly AccessControlList acl;

		protected AclPermittedContract(Address permissionManager)
			: this(permissionManager, new AccessControlList())
		{
		}

		protected AclPermittedContract(
			Address permissionManager,
			AccessControlList acl)
		{
			this.acl = acl;
			this.ConfigurePermissionManager(permissionManager);
		}

		public override bool HasPermission(Address permittedAddress, Permission permission)
		{
			return this.acl.HasPermission(permittedAddress, permission, this.Address);
		}

		protected internal override bool Receive(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			this.CheckPermission(action);

			switch (action)
			{
				case AddPermissionExecutorAction permissionAction:
					this.acl.AddPermissionExecutor(
						permissionAction.PermittedAddress,
						permissionAction.Permission,
						permissionAction.NextAddress);
					return true;

				case AddPermissionNextAddressAction permissionAction:
					this.acl.AddPermissionNextAddress(
						permissionAction.PermittedAddress,
						permissionAction.Permission,
						permissionAction.NextAddress);
					return true;

				case AddPermissionAction permissionAction:
					this.HandleAddPermissionAction(permissionAction);
					return true;

				case RemovePermissionExecutorAction permissionAction:
					this.acl.RemovePermissionExecutor(
						permissionAction.PermittedAddress,
						permissionAction.Permission,
						permissionAction.NextAddress);
					return true;

				case RemovePermissionNextAddressAction permissionAction:
					this.acl.RemovePermissionNextAddress(
						permissionAction.PermittedAddress,
						permissionAction.Permission,
						permissionAction.NextAddress);
					return true;

				case RemovePermissionAction permissionAction:
					this.HandleRemovePermissionAction(permissionAction);
					return true;

				case TracingBulletAction bulletAction:
					this.ReceiveTracingBullet(bulletAction);
					return true;

				case ForwardAction forwardAction:
					this.HandleActionForward(forwardAction);
					return true;

				default:
					return this.HandleReceivedAction(action);
			}
		}

		protected virtual void ReceiveTracingBullet(TracingBulletAction action)
		{
			if (action.TracingAction.Origin == null)
			{
				action.TracingAction.Origin = action.Origin;
				action.TracingAction.Sender = action.Sender;
			}

			// TODO replace string.Empty
			Permission permission = new Permission(action.TracingAction.GetType());

			if (action.TracingAction.Target.Equals(this.Address))
			{
				this.FindBulletPaths(action);
			}
			else if (this.acl.HasPermission(action.TracingAction.Origin, permission, this.Address))
			{
				// Path Found
				var current = new TracingElement(this.Address, action.Predecessors);
				TracingElement a = action.BfsAddresses.FirstOrDefault(x => x.Equals(current));

				if (a == null)
				{
					return;
				}

				a.IsWay = true;
				a.Way.Push(this.Address);
			}
			else
			{
				this.GetAllowedForForwarding(action, ref action.BfsAddresses);
			}
		}

		protected virtual void FindBulletPaths(TracingBulletAction action)
		{
			var bfsAddresses = new List<TracingElement>();

			this.GetAllowedForForwarding(action, ref bfsAddresses);
			for (int i = 0; i < bfsAddresses.Count; i++)
			{
				TracingElement couple = bfsAddresses.Skip(i).First();
				Stack<Address> predecessors = couple.Way ?? new Stack<Address>(new[] {this.Address});
				var newAction = new TracingBulletAction(
					couple.Address,
					action.TracingAction,
					null,
					predecessors,
					ref bfsAddresses);
				this.OnSend(newAction);
			}

			List<Stack<Address>> addressQuery = bfsAddresses.Where(x => x.IsWay).Select(x => x.Way).ToList();
			action.CallBack(addressQuery, action.TracingAction);
		}

		protected virtual List<TracingElement> GetAllowedForForwarding(
			TracingBulletAction action,
			ref List<TracingElement> queue)
		{
			var permission = new Permission(action.TracingAction.GetType());
			foreach (var address in this.acl.GetPermittedAddresses(permission, this.Address))
			{
				var predecessors = new Stack<Address>();
				if (action.Predecessors != null)
				{
					predecessors = new Stack<Address>(action.Predecessors.Reverse());
				}

				if (!predecessors.Contains(this.Address))
				{
					queue.Add(new TracingElement(address, predecessors));
				}

				predecessors.Push(this.Address);
			}

			return queue;
		}

		protected internal abstract void BulletTaken(List<Stack<Address>> ways, Action targetAction);

		protected override void HandleAddPermissionAction(AddPermissionAction action)
		{
			this.acl.AddPermission(action.PermittedAddress, action.Permission, action.NextAddress);
		}

		protected override void HandleRemovePermissionAction(RemovePermissionAction action)
		{
			this.acl.RemovePermission(action.PermittedAddress, action.Permission, action.NextAddress);
		}

		private bool CheckPermission(Action action)
		{
			var permission = new Permission(action.GetType());
			if (!this.acl.HasPermission(action.Sender, permission, this.Address))
			{
				throw new NoPermissionException(this, action.Sender, permission);
			}

			return true;
		}

		private void HandleActionForward(ForwardAction forwardAction)
		{
			if (forwardAction?.Target == null)
			{
				throw new ArgumentNullException(nameof(forwardAction));
			}

			if (forwardAction.ForwardedAction.Target.Equals(this.Address))
			{
				this.Receive(forwardAction.ForwardedAction);
			}
			else
			{
				Address target = forwardAction.WayForForwarding.Pop();
				this.OnForward(forwardAction.ForwardedAction, target, forwardAction.WayForForwarding);
			}
		}

		private void ConfigurePermissionManager(Address permissionManager)
		{
			this.acl.AddPermission(
				permissionManager,
				new Permission(typeof(AddPermissionAction)),
				this.Address);
			this.acl.AddPermission(
				permissionManager,
				new Permission(typeof(RemovePermissionAction)),
				this.Address);
			this.acl.AddPermission(
				permissionManager,
				new Permission(typeof(AddPermissionExecutorAction)),
				this.Address);
			this.acl.AddPermission(
				permissionManager,
				new Permission(typeof(RemovePermissionExecutorAction)),
				this.Address);
			this.acl.AddPermission(
				permissionManager,
				new Permission(typeof(AddPermissionNextAddressAction)),
				this.Address);
			this.acl.AddPermission(
				permissionManager,
				new Permission(typeof(RemovePermissionNextAddressAction)),
				this.Address);
		}
	}
}