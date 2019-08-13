using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using StrongForce.Core.Exceptions;
using StrongForce.Core.Permissions;

namespace StrongForce.Core
{
	public abstract class Contract
	{
		[ThreadStatic]
		internal static Address CurrentlyCreatingAddress;

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
				AddPermissionAction.Type,
				this.Address);
			this.Acl.AddPermission(
				initialAdmin,
				RemovePermissionAction.Type,
				this.Address);
		}

		internal event System.Action<Address[], string, IDictionary<string, object>> SendActionEvent;

		internal event System.Action<ulong> ForwardActionEvent;

		internal event Func<Type, object[], Address> CreateContractEvent;

		public Address Address { get; internal set; } = null;

		public AccessControlList Acl { get; set; } = new AccessControlList();

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

			this.CheckPermission(action); // throws

			if (action is PayloadAction payloadAction)
			{
				return this.HandlePayloadAction(payloadAction);
			}
			else if (action is ForwardAction forwardAction)
			{
				return this.HandleForwardAction(forwardAction);
			}
			else
			{
				return false;
			}
		}

		protected virtual void CheckPermission(Action action)
		{
			var neededPermission = new Permission(action.Type, action.Sender, action.FinalTarget);

			if (!this.Acl.HasPermission(neededPermission))
			{
				throw new NoPermissionException(this.Address, action.Origin, neededPermission);
			}
		}

		protected virtual bool HandlePayloadAction(PayloadAction action)
		{
			switch (action.Type)
			{
				case AddPermissionAction.Type:
					this.Acl.AddPermission(AddPermissionAction.GetPermission(action));
					return true;

				case RemovePermissionAction.Type:
					this.Acl.RemovePermission(RemovePermissionAction.GetPermission(action));
					return true;

				default:
					return false;
			}
		}

		protected virtual bool HandleForwardAction(ForwardAction action)
		{
			this.ForwardAction(action.NextId);
			return true;
		}

		protected void SendAction(Address[] targets, string type, IDictionary<string, object> payload)
		{
			this.SendActionEvent?.Invoke(targets, type, payload);
		}

		protected void SendAction(Address target, string type, IDictionary<string, object> payload)
		{
			this.SendAction(new[] { target }, type, payload);
		}

		protected void ForwardAction(ulong id)
		{
			this.ForwardActionEvent?.Invoke(id);
		}

		protected Address CreateContract(Type contractType, object[] initialState)
		{
			if (contractType == null)
			{
				throw new ArgumentNullException();
			}

			return this.CreateContractEvent?.Invoke(contractType, initialState);
		}

		protected Address CreateContract<T>(object[] initialState)
		{
			return this.CreateContract(typeof(T), initialState);
		}
	}
}