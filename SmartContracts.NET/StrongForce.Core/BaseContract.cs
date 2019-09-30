using System;
using System.Collections.Generic;
using StrongForce.Core.Exceptions;
using StrongForce.Core.Permissions;

namespace StrongForce.Core
{
	public abstract class BaseContract : StatefulObject
	{
		public BaseContract()
		{
		}

		public Address Address { get => this.Context.Address; }

		protected IContractContext Context { get; private set; }

		public Action<Message> RegisterWithRegistry(IContractContext context)
		{
			if (this.Context != null)
			{
				throw new InvalidOperationException("Already registered with registry");
			}

			this.Context = context;
			this.Initialize();
			return this.Receive;
		}

		protected void Receive(Message message)
		{
			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			this.CheckPermissions(message);

			if (message is ForwardMessage forwardMessage)
			{
				this.HandleForwardMessage(forwardMessage);
			}
			else
			{
				this.HandleMessage(message);
			}
		}

		protected abstract void CheckPermissions(Message message);

		protected abstract void HandleMessage(Message message);

		protected abstract void HandleForwardMessage(ForwardMessage message);

		protected virtual void Initialize()
		{
		}

		protected void SendMessage(Address[] targets, string type, IDictionary<string, object> payload)
		{
			this.Context.SendMessage(targets, type, payload);
		}

		protected void SendMessage(Address target, string type, IDictionary<string, object> payload)
		{
			this.SendMessage(new[] { target }, type, payload);
		}

		protected void ForwardMessage(ulong id)
		{
			this.Context.ForwardMessage(id);
		}

		protected Address CreateContract<T>(IDictionary<string, object> payload)
			where T : BaseContract, new()
		{
			return this.Context.CreateContract<T>(payload);
		}
	}
}