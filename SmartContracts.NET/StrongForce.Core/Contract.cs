using System;
using System.Collections.Generic;
using StrongForce.Core.Exceptions;
using StrongForce.Core.Extensions;
using StrongForce.Core.Permissions;

namespace StrongForce.Core
{
	public class Contract : BaseContract
	{
		public AccessControlList Acl { get; } = new AccessControlList();

		public override IDictionary<string, object> GetState()
		{
			var state = base.GetState();

			state.Set("Acl", this.Acl.GetState());

			return state;
		}

		protected override void SetState(IDictionary<string, object> state)
		{
			this.Acl.SetState(state.Get<IDictionary<string, object>>("Acl"));

			base.SetState(state);
		}

		protected override void Initialize(IDictionary<string, object> payload)
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

			this.SetState(this.GetState().MergeStateWith(payload));
		}

		protected override void CheckPermissions(Message message)
		{
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