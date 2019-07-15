using System;
using System.Collections.Generic;

namespace StrongForce.Core
{
	public abstract class Contract
	{
		protected Contract(Address address)
		{
			this.Address = address;
		}

		internal event EventHandler<ActionEventArgs> Send;

		internal event EventHandler<ActionEventArgs> Forward;

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
			ActionEventArgs e = new ActionEventArgs(action);
			this.Send?.Invoke(this, e);
		}

		protected virtual void ForwardAction(Action action, Address target, Stack<Address> ways)
		{
			ForwardAction forwarded = new ForwardAction(target, action, ways);
			ActionEventArgs e = new ActionEventArgs(forwarded);
			this.Forward?.Invoke(this, e);
		}
	}
}