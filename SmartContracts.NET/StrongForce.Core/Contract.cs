using System;
using System.Collections.Generic;
using StrongForce.Core.Permissions.Actions;

namespace StrongForce.Core
{
	public abstract class Contract
	{
		protected Contract(Address address)
		{
			this.Address = address;
		}


		public Address Address { get; }

		internal ContractRegistry Registry { private get; set; }

		internal virtual bool Receive(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return this.HandleAction(action);
		}

		protected abstract object GetState();

		protected virtual bool HandleAction(Action action)
		{
			return false;
		}

		protected void SendAction(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException();
			}

			action.Sender = this.Address;
			action.Origin = this.Address;
			this.Registry.HandleAction(action);
		}

		protected void SendAction(ForwardAction action)
		{
			if (action == null)
			{
				throw new ArgumentNullException();
			}

			if (action.Origin == null)
			{
				action.Origin = this.Address;
			}

			action.Sender = this.Address;
			this.Registry.HandleAction(action);
		}
	}
}