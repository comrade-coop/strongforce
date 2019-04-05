using System;
using System.Collections.Generic;
using ContractsCore.Actions;
using ContractsCore.Events;
using Action = ContractsCore.Actions.Action;

namespace ContractsCore.Contracts
{
	public abstract class Contract
	{
		protected Contract(Address address, ContractRegistry registry)
		{
			this.Address = address;
			this.Registry = registry;
		}

		protected ContractRegistry Registry;

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

			return this.HandleAcceptedAction(action);
		}

		protected abstract void ReceiveTracingBullet(TracingBulletAction action);

		protected abstract List<TracingElement> GetAllowedForForwording(TracingBulletAction action,
			ref List<TracingElement> queue);

		protected internal abstract void BulletTaken(List<Stack<Address>> ways, Action targetAction);

		protected abstract bool HandleAcceptedAction(Action action);

		protected virtual void Redirect(Action action)
		{
		}

		protected virtual void OnSend(Action action)
		{
			ActionEventArgs e = new ActionEventArgs(action);
			this.Send?.Invoke(this, e);
		}

		protected virtual void OnForward(Action action, Address target, Stack<Address> ways)
		{
			ForwardAction forwarded = new ForwardAction(string.Empty, target, action, ways);
			ActionEventArgs e = new ActionEventArgs(forwarded);
			this.Forward?.Invoke(this, e);
		}
	}
}