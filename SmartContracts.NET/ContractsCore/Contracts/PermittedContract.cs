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
	public abstract class PermittedContract : Contract
	{
		private readonly AccessControlList acl;

		protected PermittedContract(Address address, ContractRegistry registry, Address permissionManager)
			: this(address, registry, permissionManager, new AccessControlList())
		{
		}

		protected PermittedContract(Address address, ContractRegistry registry, Address permissionManager, AccessControlList acl)
			: base(address, registry)
		{
			this.acl = acl;
			this.ConfigurePermissionManager(permissionManager);
		}

		protected bool HasPermission(Address address, Permission permission, Address target)
		{
			return this.acl.HasPermission(address, permission, target);
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
				case AddPermissionAction permissionAction:
					this.HandleAddPermissionAction(permissionAction);
					return true;

				case RemovePermissionAction permissionAction:
					this.HandleRemovePermissionAction(permissionAction);
					return true;

				case TracingBulletAction bulletAction:
					this.ReceiveTracingBullet(bulletAction);
					return true;

				case ForwardAction forwardAction:
					// TODO extract in function
					if (forwardAction.ForwardedAction.Target.Equals(this.Address) )
					{
						return this.Receive(forwardAction.ForwardedAction);
					}
					else
					{
						Address target = forwardAction.WayForForwarding.Pop();
						this.OnForward(forwardAction.ForwardedAction, target, forwardAction.WayForForwarding);
						return true;
					}

				default:
					return this.HandleAcceptedAction(action);
			}
		}

		protected override void ReceiveTracingBullet(TracingBulletAction action)
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
				this.GetAllowedForForwording(action, ref action.BfsAddresses);
			}
		}

		protected void FindBulletPaths(TracingBulletAction action)
		{
			List<TracingElement> bfsAddresses = new List<TracingElement>();

			this.GetAllowedForForwording(action, ref bfsAddresses);
			for (int i = 0; bfsAddresses.Count > i; i++)
			{
				TracingElement couple = bfsAddresses.Skip(i).First();
				Contract currentContract = this.Registry.GetContract(couple.Address);
				Stack<Address> predecessors = couple.Way ?? new Stack<Address>(new[] { this.Address });
				TracingBulletAction newAction = new TracingBulletAction(string.Empty, couple.Address, action.TracingAction,
					null, predecessors, ref bfsAddresses);
				this.OnSend(newAction);
			}

			List<Stack<Address>> addressQuery = bfsAddresses.Where(x => x.IsWay).Select(x => x.Way).ToList();
			action.CallBack(addressQuery, action.TracingAction);
		}

		protected override List<TracingElement> GetAllowedForForwording(TracingBulletAction action,
			ref List<TracingElement> queue)
		{
			var permission = new Permission(action.TracingAction.GetType());
			foreach (var address in this.acl.GetPermittedAddresses(permission, this.Address))
			{
				Stack<Address> predecessors = action.Predecessors ?? new Stack<Address>();
				predecessors.Push(this.Address);
				queue.Add(new TracingElement(address, predecessors, false));
			}

			return queue;
		}

		/* protected override void BulletTaken(List<List<Address>> ways, Action targaetAction)
		{

		}*/

		private void ConfigurePermissionManager(Address permissionManager)
		{
			this.acl.AddPermission(permissionManager, new Permission(typeof(AddPermissionAction)), this.Address);
			this.acl.AddPermission(permissionManager, new Permission(typeof(RemovePermissionAction)), this.Address);
		}

		private void CheckPermission(Action action)
		{
			var permission = new Permission(action.GetType());
			if (!this.acl.HasPermission(action.Sender, permission, this.Address))
			{
				throw new NoPermissionException(this, action.Sender, permission);
			}
		}

		private void HandleAddPermissionAction(AddPermissionAction action)
		{
			this.acl.AddPermission(action.PermittedAddress, action.Permission, action.NextAddress);
		}

		private void HandleRemovePermissionAction(RemovePermissionAction action)
		{
			this.acl.RemovePermission(action.PermittedAddress, action.Permission, action.NextAddress);
		}

	}
}