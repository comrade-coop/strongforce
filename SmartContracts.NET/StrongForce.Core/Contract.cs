using System;
using System.Collections.Generic;
using StrongForce.Core.Exceptions;
using StrongForce.Core.Extensions;
using StrongForce.Core.Permissions;

namespace StrongForce.Core
{
	public class Contract : BaseContract
	{
		private IDictionary<string, object> initialState = null;

		public AccessControlList Acl { get; protected set; } = new AccessControlList();

		public override IDictionary<string, object> GetState()
		{
			var state = base.GetState();

			state.Set("Acl", this.Acl.GetState());

			return state;
		}

		protected override void SetState(IDictionary<string, object> state)
		{
			this.initialState = state;

			this.Acl = StatefulObject.CreateFromState<AccessControlList>(state.GetDictionary("Acl"));

			base.SetState(state);
		}

		protected override void Initialize()
		{
			if (this.initialState != null)
			{
				this.Initialize(this.initialState);
			}

			this.initialState = null;
		}

		protected virtual void Initialize(IDictionary<string, object> payload)
		{
			if (payload.ContainsKey("Admin"))
			{
				var admin = payload.Get<Address>("Admin");

				this.Acl.AddPermission(
					admin,
					AddPermissionAction.Type,
					this.Address);
				this.Acl.AddPermission(
					admin,
					RemovePermissionAction.Type,
					this.Address);
			}
		}

		protected override void CheckPermissions(Message message)
		{
			if (this.initialState != null)
			{
				throw new InvalidOperationException();
			}

			var neededPermission = new Permission(message.Type, message.Sender, message.FinalTarget);

			if (!this.Acl.HasPermission(neededPermission))
			{
				throw new NoPermissionException(this.Address, message.Origin, neededPermission);
			}
		}

		protected override void HandleMessage(Message message)
		{
			switch (message.Type)
			{
				case AddPermissionAction.Type:
					this.Acl.AddPermission(AddPermissionAction.GetPermission(message.Payload));
					break;

				case RemovePermissionAction.Type:
					this.Acl.RemovePermission(RemovePermissionAction.GetPermission(message.Payload));
					break;
			}
		}

		protected override void HandleForwardMessage(ForwardMessage message)
		{
			this.ForwardMessage(message.ForwardId);
		}
	}
}