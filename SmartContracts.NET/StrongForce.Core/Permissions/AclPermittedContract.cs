using System;
using System.Collections.Generic;
using System.Linq;
using StrongForce.Core.Exceptions;

namespace StrongForce.Core.Permissions
{
	public abstract class AclPermittedContract : PermittedContract
	{
		protected AclPermittedContract(Address address, Address permissionManager)
			: this(address, permissionManager, new AccessControlList())
		{
		}

		protected AclPermittedContract(Address address, Address permissionManager, AccessControlList acl)
			: base(address)
		{
			this.Acl = acl;
			this.ConfigurePermissionManager(permissionManager);
		}

		protected AccessControlList Acl { get; set; }

		protected override bool HandleAction(Action action)
		{
			switch (action)
			{
				case UpdatePermissionAction permissionAction:
					this.HandleUpdatePermissionAction(permissionAction);
					return true;

				case TracingBulletAction bulletAction:
					this.ReceiveTracingBullet(bulletAction);
					return true;

				case ForwardAction forwardAction:
					this.HandleActionForward(forwardAction);
					return true;

				default:
					return base.HandleAction(action);
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
				this.ForwardAction(forwardAction.ForwardedAction, target, forwardAction.WayForForwarding);
			}
		}

		protected override void ReceiveTracingBullet(TracingBulletAction action)
		{
			if (action.TracingAction.Origin == null)
			{
				action.TracingAction.Origin = action.Origin;
				action.TracingAction.Sender = action.Sender;
			}

			Permission permission = new Permission(action.TracingAction.GetType());

			if (action.TracingAction.Target == this.Address)
			{
				this.FindBulletPaths(action);
			}
			else if (this.Acl.HasPermission(action.TracingAction.Origin, permission, this.Address))
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

				Stack<Address> predecessors = couple.Way ?? new Stack<Address>(new[] { this.Address });
				TracingBulletAction newAction = new TracingBulletAction(
					couple.Address,
					action.TracingAction,
					null,
					predecessors,
					ref bfsAddresses);
				this.SendAction(newAction);
			}

			List<Stack<Address>> addressQuery = bfsAddresses.Where(x => x.IsWay).Select(x => x.Way).ToList();
			action.CallBack(addressQuery, action.TracingAction);
		}

		protected virtual List<TracingElement> GetAllowedForForwarding(TracingBulletAction action, ref List<TracingElement> queue)
		{
			var permission = new Permission(action.TracingAction.GetType());
			foreach (var address in this.Acl.GetPermittedAddresses(permission, this.Address))
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

		protected override bool CheckPermission(Action action)
		{
			var permission = new Permission(action.GetType());
			if (!this.Acl.HasPermission(action.Sender, permission, this.Address))
			{
				throw new NoPermissionException(this, action.Sender, permission);
			}

			return true;
		}

		protected override void BulletTaken(List<Stack<Address>> ways, Action targetAction)
		{
			throw new NotImplementedException();
		}

		protected override void HandleAddPermissionAction(AddPermissionAction action)
		{
			this.Acl.AddPermission(action.PermittedAddress, action.Permission, action.Receiver);
		}

		protected override void HandleRemovePermissionAction(RemovePermissionAction action)
		{
			this.Acl.RemovePermission(action.PermittedAddress, action.Permission, action.Receiver);
		}

		protected void HandleUpdatePermissionAction(UpdatePermissionAction action)
		{
			this.Acl.UpdatePermission(action.OldPermittedAddress, action.OldReceiver, action.Permission, action.NewPermittedAddress, action.NewReceiver);
		}

		private void ConfigurePermissionManager(Address permissionManager)
		{
			this.Acl.AddPermission(permissionManager, new Permission(typeof(AddPermissionAction)), this.Address);
			this.Acl.AddPermission(permissionManager, new Permission(typeof(RemovePermissionAction)), this.Address);
			this.Acl.AddPermission(permissionManager, new Permission(typeof(UpdatePermissionAction)), this.Address);
		}
	}
}