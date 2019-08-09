using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using StrongForce.Core.Exceptions;
using StrongForce.Core.Permissions;

namespace StrongForce.Core
{
	public abstract class Contract
	{
		public Contract()
		{
			// HACK: Needed so that the other constructors knows about the "self" address
			if (CurrentlyCreatingAddress != null)
			{
				this.Address = CurrentlyCreatingAddress;
				CurrentlyCreatingAddress = null;
			}
		}

		public Contract(Address initialAdmin)
			: this()
		{
			this.Acl.AddPermission(
				initialAdmin,
				typeof(AddPermissionAction),
				this.Address);
			this.Acl.AddPermission(
				initialAdmin,
				typeof(RemovePermissionAction),
				this.Address);
			this.Acl.AddPermission(
				initialAdmin,
				typeof(RemovePermissionAction),
				this.Address);
		}

		internal event System.Action<Action> SendActionEvent;

		internal event Func<Type, object[], Address> CreateContractEvent;

		public Address Address { get; internal set; } = null;

		public AccessControlList Acl { get; set; } = new AccessControlList();

		internal static Address CurrentlyCreatingAddress { get; set; } = null;

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
				throw new NoPermissionException(this, context.Sender, action.GetType());
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

			var permission = checkedAction.GetType();
			return this.Acl.HasPermission(context.Sender, permission, checkedAction.Target);
		}

		protected virtual bool HandleAction(ActionContext context, Action action)
		{
			switch (action)
			{
				case AddPermissionAction permissionAction:
					this.Acl.AddPermission(permissionAction.Permission);
					return true;

				case RemovePermissionAction permissionAction:
					this.Acl.RemovePermission(permissionAction.Permission);
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

		protected Address CreateContract(Type contractType, params object[] constructorParameters)
		{
			if (contractType == null)
			{
				throw new ArgumentNullException();
			}

			return this.CreateContractEvent?.Invoke(contractType, constructorParameters);
		}

		protected Address CreateContract<T>(params object[] constructorParameters)
		{
			return this.CreateContract(typeof(T), constructorParameters);
		}
	}
}