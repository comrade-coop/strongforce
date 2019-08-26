using System;
using System.Collections.Generic;
using StrongForce.Core.Exceptions;
using StrongForce.Core.Extensions;
using StrongForce.Core.Permissions;

namespace StrongForce.Core
{
	public abstract class BaseContract : IStatefulObject
	{
		public BaseContract()
		{
		}

		public Address Address { get; private set; } = null;

		private ContractHandlers ContractHandlers { get; set; }

		public static (BaseContract, Action<Message>) Create(Type contractType, Address address, IDictionary<string, object> payload, ContractHandlers contractHandlers, bool isDeserialization = false)
		{
			if (!typeof(BaseContract).IsAssignableFrom(contractType))
			{
				throw new ArgumentOutOfRangeException(nameof(contractType));
			}

			var contract = (BaseContract)Activator.CreateInstance(contractType);

			contract.Address = address;
			contract.ContractHandlers = contractHandlers;

			if (isDeserialization)
			{
				contract.SetState(payload);
			}
			else
			{
				contract.Initialize(payload);
			}

			return (contract, contract.Receive);
		}

		public virtual IDictionary<string, object> GetState()
		{
			return new Dictionary<string, object>();
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

		protected virtual void SetState(IDictionary<string, object> state)
		{
		}

		protected virtual void Initialize(IDictionary<string, object> payload)
		{
			this.SetState(this.GetState().MergeStateWith(payload));
		}

		protected abstract void CheckPermissions(Message message);

		protected abstract void HandleMessage(Message message);

		protected abstract void HandleForwardMessage(ForwardMessage message);

		protected void SendMessage(Address[] targets, string type, IDictionary<string, object> payload)
		{
			this.ContractHandlers.SendMessage.Invoke(targets, type, payload);
		}

		protected void SendMessage(Address target, string type, IDictionary<string, object> payload)
		{
			this.SendMessage(new[] { target }, type, payload);
		}

		protected void ForwardMessage(ulong id)
		{
			this.ContractHandlers.ForwardMessage.Invoke(id);
		}

		protected Address CreateContract(Type contractType, IDictionary<string, object> payload)
		{
			return this.ContractHandlers.CreateContract.Invoke(contractType, payload);
		}

		protected Address CreateContract<T>(IDictionary<string, object> payload)
		{
			return this.CreateContract(typeof(T), payload);
		}
	}
}