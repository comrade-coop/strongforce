using System;
using System.Collections.Generic;
using StrongForce.Core.Exceptions;
using StrongForce.Core.Extensions;
using StrongForce.Core.Permissions;

namespace StrongForce.Core
{
	public abstract class Contract : IStateObject
	{
		public Contract()
		{
		}

		// Events used to communicate with the registry
		internal event System.Action<Address[], string, IDictionary<string, object>> SendActionEvent;

		internal event System.Action<Address, string, IDictionary<string, object>> SendEventEvent;

		internal event System.Action<ulong> ForwardActionEvent;

		internal event Func<Type, IDictionary<string, object>, Address> CreateContractEvent;

		public Address Address { get; private set; } = null;

		public AccessControlList Acl { get; } = new AccessControlList();

		public virtual IDictionary<string, object> GetState()
		{
			var state = new Dictionary<string, object>();

			state.Add("Acl", this.Acl.GetState());

			return state;
		}

		public virtual void SetState(IDictionary<string, object> state)
		{
			this.Acl.SetState(state.GetDictionary("Acl"));
		}

		internal void Configure(Address address, IDictionary<string, object> payload)
		{
			this.Address = address;
			if (payload != null)
			{
				this.Initialize(payload);
			}
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
			else if (action is EventAction eventAction)
			{
				return this.HandleEventAction(eventAction);
			}
			else
			{
				return false;
			}
		}

		protected virtual void Initialize(IDictionary<string, object> payload)
		{
			var admin = payload.GetAddress("Admin");

			this.Acl.AddPermission(
				admin,
				AddPermissionAction.Type,
				this.Address);
			this.Acl.AddPermission(
				admin,
				RemovePermissionAction.Type,
				this.Address);
		}

		protected virtual void CheckPermission(Action action)
		{
			if (action is EventAction)
			{
				return; // No permissions needed for events
			}

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

		protected virtual bool HandleEventAction(EventAction action)
		{
			return false;
		}

		protected void SendAction(Address[] targets, string type, IDictionary<string, object> payload)
		{
			this.SendActionEvent?.Invoke(targets, type, payload);
		}

		protected void SendAction(Address target, string type, IDictionary<string, object> payload)
		{
			this.SendAction(new[] { target }, type, payload);
		}

		protected void SendEvent(Address target, string type, IDictionary<string, object> payload)
		{
			this.SendEventEvent?.Invoke(target, type, payload);
		}

		protected void ForwardAction(ulong id)
		{
			this.ForwardActionEvent?.Invoke(id);
		}

		protected Address CreateContract(Type contractType, IDictionary<string, object> payload)
		{
			return this.CreateContractEvent?.Invoke(contractType, payload);
		}

		protected Address CreateContract<T>(IDictionary<string, object> payload)
		{
			return this.CreateContract(typeof(T), payload);
		}
	}
}