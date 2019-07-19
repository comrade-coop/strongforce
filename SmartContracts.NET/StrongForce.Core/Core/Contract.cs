using System;
using System.Collections.Generic;

namespace StrongForce.Core
{
	public abstract class Contract
	{
		internal ContractRegistry Registry;

		protected Contract(Address address)
		{
			this.Address = address;
		}

		public Address Address { get; }

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

		protected virtual void SendAction(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException();
			}

			action.Sender = this.Address;
			action.Origin = this.Address;
			this.Registry.HandleAction(action);
		}

		protected virtual void ForwardAction(Action action, Address target, Stack<Address> ways)
		{
			action.Sender = this.Address;
			ForwardAction forwarded = new ForwardAction(target, action, ways)
			{
				Sender = this.Address,
				Origin = this.Address,
			};
			this.Registry.HandleAction(forwarded);
		}
	}
}