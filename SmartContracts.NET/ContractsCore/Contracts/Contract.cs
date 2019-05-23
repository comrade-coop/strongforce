using System;
using System.Collections.Generic;
using ContractsCore.Actions;
using ContractsCore.Events;
using Action = ContractsCore.Actions.Action;

namespace ContractsCore.Contracts
{
	public abstract class Contract
	{
		private readonly IContractRegistry contractRegistry = ContractRegistry.GetInstance();

		protected Contract()
		{
			this.contractRegistry.RegisterContract(this);
		}

		internal event EventHandler<ActionEventArgs> Send;

		internal event EventHandler<ActionEventArgs> Forward;

		public Address Address { get; internal set; }

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

		protected virtual void OnSend(Action action)
		{
			ActionEventArgs e = new ActionEventArgs(action);
			this.Send?.Invoke(this, e);
		}

		protected virtual void OnForward(Action action, Address target, Stack<Address> ways)
		{
			var forwarded = new ForwardAction(target, action, ways);
			var e = new ActionEventArgs(forwarded);
			this.Forward?.Invoke(this, e);
		}
	}
}