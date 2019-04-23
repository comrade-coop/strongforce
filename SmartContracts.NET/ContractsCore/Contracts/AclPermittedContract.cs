using System;
using System.Collections.Generic;
using System.Linq;
using ContractsCore.Actions;
using ContractsCore.Events;
using ContractsCore.Exceptions;
using ContractsCore.Permissions;
using Action = ContractsCore.Actions.Action;

namespace ContractsCore.Contracts
{
	public abstract class AclPermittedContract : PermittedContract
	{
		protected readonly AccessControlList acl;

		protected readonly ContractRegistry registry;

		protected AclPermittedContract(Address address, ContractRegistry registry, Address permissionManager)
			: this(address, registry, permissionManager, new AccessControlList())
		{
		}

		protected AclPermittedContract(Address address, ContractRegistry registry, Address permissionManager, AccessControlList acl)
			: base(address)
		{
			this.acl = acl;
			this.registry = registry;
			this.ConfigurePermissionManager(permissionManager);
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
					this.acl.AddPermissionExecutor(permissionAction.PermittedAddress, permissionAction.Permission, permissionAction.NextAddress);
					return true;

				case AddPermissionNextAddressAction permissionAction:
					this.acl.AddPermissionNextAddress(permissionAction.PermittedAddress, permissionAction.Permission, permissionAction.NextAddress);
					return true;

				case AddPermissionAction permissionAction:
					this.HandleAddPermissionAction(permissionAction);
					return true;

				case RemovePermissionExecutorAction permissionAction:
					this.acl.RemovePermissionExecutor(permissionAction.PermittedAddress, permissionAction.Permission, permissionAction.NextAddress);
					return true;

				case RemovePermissionNextAddressAction permissionAction:
					this.acl.RemovePermissionNextAddress(permissionAction.PermittedAddress, permissionAction.Permission, permissionAction.NextAddress);
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

		protected void HandleActionForward(ForwardAction forwardAction)
		{
			if (forwardAction == null || forwardAction.Target == null)
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

		protected virtual void ReceiveTracingBullet(TracingBulletAction action)
		{
			if (action.TracingAction.Origin == null)
			{
				action.TracingAction.Origin = action.Origin;
				action.TracingAction.Sender = action.Sender;
			}
			// TODO replace string.Empty
			Permission permission = new Permission(action.TracingAction.GetType());

			if (action.TracingAction.Target == this.Address)
			{
				this.FindBulletPaths(action);
			}
			else if (this.acl.HasPermission(action.TracingAction.Origin, permission, this.Address))
			{ // Path Found
				TracingElement current = new TracingElement(this.Address, action.Predecessors);
				var a = action.BfsAddresses.FirstOrDefault(x => x.Equals(current));
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
				Contract currentContract = this.registry.GetContract(couple.Address);
				Stack<Address> predecessors = couple.Way ?? new Stack<Address>(new[] { this.Address });
				TracingBulletAction newAction = new TracingBulletAction(string.Empty, couple.Address, action.TracingAction,
					null, predecessors, ref bfsAddresses);
				this.OnSend(newAction);
			}

			List<Stack<Address>> addressQuery = bfsAddresses.Where(x => x.IsWay).Select(x => x.Way).ToList();
			action.CallBack(addressQuery, action.TracingAction);
		}

		protected virtual List<TracingElement> GetAllowedForForwarding(TracingBulletAction action,
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
					queue.Add(new TracingElement(address, predecessors, false));
				}

				predecessors.Push(this.Address);
			}

			return queue;
		}

		protected internal abstract void BulletTaken(List<Stack<Address>> ways, Action targetAction);

		private void ConfigurePermissionManager(Address permissionManager)
		{
			this.acl.AddPermission(permissionManager, new Permission(typeof(AddPermissionAction)), this.Address);
			this.acl.AddPermission(permissionManager, new Permission(typeof(RemovePermissionAction)), this.Address);
		}

		protected override bool CheckPermission(Action action)
		{
			var permission = new Permission(action.GetType());
			if (!this.acl.HasPermission(action.Sender, permission, this.Address))
			{
				throw new NoPermissionException(this, action.Sender, permission);
			}

			return true;
		}

		protected override void HandleAddPermissionAction(AddPermissionAction action)
		{
			this.acl.AddPermission(action.PermittedAddress, action.Permission, action.NextAddress);
		}

		protected override void HandleRemovePermissionAction(RemovePermissionAction action)
		{
			this.acl.RemovePermission(action.PermittedAddress, action.Permission, action.NextAddress);
		}

	}
}