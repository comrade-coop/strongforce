using System;
using ContractsCore.Events;
using Action = ContractsCore.Actions.Action;

namespace ContractsCore.Contracts
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

		protected internal abstract object GetState();

		protected internal virtual bool Receive(Action action)
		{
			if (action == null)
			{
				throw new ArgumentNullException(nameof(action));
			}

			return this.HandleReceivedAction(action);
		}

		protected abstract bool HandleReceivedAction(Action action);

		protected virtual void Redirect(Action action)
		{
		}

		protected virtual void OnSend(ActionEventArgs e)
		{
			this.Send?.Invoke(this, e);
		}

		protected virtual void OnForward(ActionEventArgs e)
		{
			this.Forward?.Invoke(this, e);
		}
	}
}