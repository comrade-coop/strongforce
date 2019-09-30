using System;
using System.Collections.Generic;
using System.Linq;
using StrongForce.Core.Exceptions;
using StrongForce.Core.Extensions;
using StrongForce.Core.Serialization;

namespace StrongForce.Core
{
	public class ContractRegistry : StatefulObject
	{
		protected IContractRegistryContext Context { get; set; }

		protected BaseAddressFactory AddressFactory { get; set; } = new HashedAddressFactory();

		protected IDictionary<ulong, Message> ForwardedMessages { get; set; } = new Dictionary<ulong, Message>();

		protected ulong MessageNonce { get; set; } = 0;

		protected bool Errored { get; set; }

		protected IDictionary<Address, (BaseContract, Action<Message>)> CachedContracts { get; } = new Dictionary<Address, (BaseContract, Action<Message>)>();

		public Action<Address, Address[], string, IDictionary<string, object>> SetContext(IContractRegistryContext context)
		{
			if (this.Context != null)
			{
				throw new InvalidOperationException("Context has already been set");
			}

			this.Context = context;

			return this.ReceiveMessage;
		}

		public override IDictionary<string, object> GetState()
		{
			var state = new Dictionary<string, object>();

			state.Set("AddressFactory", new Dictionary<string, object>()
			{
				{ "Type", this.AddressFactory.GetType().AssemblyQualifiedName },
				{ "State", this.AddressFactory.GetState() },
			});

			state.Set("ForwardedMessages", this.ForwardedMessages.ToDictionary(
				kv => (string)kv.Key.ToString(),
				kv => (object)kv.Value.GetState()));

			state.Set("MessageNonce", this.MessageNonce.ToString());

			if (this.Context != null)
			{
				foreach (var contract in this.CachedContracts)
				{
					this.Context.SaveContract(contract.Value.Item1);
				}
			}

			return state;
		}

		protected override void SetState(IDictionary<string, object> state)
		{
			var factory = state.GetDictionary("AddressFactory");
			var factoryType = Type.GetType(factory.Get<string>("Type"));
			var factoryState = factory.GetDictionary("State");

			this.AddressFactory = (BaseAddressFactory)StatefulObject.Create(factoryType, factoryState);

			this.ForwardedMessages = state.GetDictionary("ForwardedMessages").ToDictionary(
				kv => ulong.Parse(kv.Key),
				kv => Message.Create((IDictionary<string, object>)kv.Value));

			this.MessageNonce = ulong.Parse(state.Get<string>("MessageNonce"));
		}

		protected void ReceiveMessage(Address sender, Address[] targets, string type, IDictionary<string, object> payload)
		{
			this.Errored = false;

			this.SendMessage(sender, targets, type, payload);
		}

		protected void SendMessage(Address sender, Address[] targets, string type, IDictionary<string, object> payload)
		{
			var message = this.CreateMessage(sender, sender, targets, type, payload);

			this.DispatchMessage(message);
		}

		protected void SendMessage(Address sender, ulong id)
		{
			var message = this.ForwardedMessages[id];

			if (message.Sender != sender)
			{
				throw new ArgumentOutOfRangeException(nameof(sender));
			}

			this.DispatchMessage(message);
		}

		protected Address CreateContract<T>(IDictionary<string, object> payload = null)
			where T : BaseContract, new()
		{
			if (payload != null && !StateSerialization.ValidateState(payload))
			{
				throw new ArgumentOutOfRangeException(nameof(payload));
			}

			var address = this.AddressFactory.CreateAddress();

			var contract = StatefulObject.Create<T>(payload);

			var receiver = contract.RegisterWithRegistry(new ContractContext(this, address));

			this.CachedContracts.Add(address, (contract, receiver));

			return address;
		}

		protected (BaseContract, Action<Message>) GetContract(Address address)
		{
			if (this.CachedContracts.TryGetValue(address, out var result))
			{
				return result;
			}
			else
			{
				var contract = this.Context.LoadContract(address);

				var receiver = contract.RegisterWithRegistry(new ContractContext(this, address));

				this.CachedContracts.Add(address, (contract, receiver));

				return (contract, receiver);
			}
		}

		private void DispatchMessage(Message message)
		{
			var (contract, receiver) = this.GetContract(message.Target);

			try
			{
				receiver.Invoke(message);
			}
			catch (Exception)
			{
				this.Errored = true;
				throw;
			}

			if (this.Errored)
			{
				throw new InvalidOperationException("Using try/catch around SendMessage is not allowed.");
			}
		}

		private Message CreateMessage(Address origin, Address sender, Address[] targets, string type, IDictionary<string, object> payload, bool eventMessage = false)
		{
			if (targets == null || targets.Length == 0)
			{
				throw new ArgumentNullException(nameof(targets));
			}

			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (payload == null)
			{
				throw new ArgumentNullException(nameof(payload));
			}

			if (!StateSerialization.ValidateState(payload))
			{
				throw new ArgumentOutOfRangeException(nameof(payload));
			}

			if (targets.Length > 1)
			{
				var nextId = this.MessageNonce;
				this.MessageNonce++;

				var nextTargets = targets.Skip(1).ToArray();

				this.ForwardedMessages.Add(nextId, this.CreateMessage(origin, targets[0], nextTargets, type, payload));

				return new ForwardMessage(targets[0], origin, sender, type, payload, nextTargets, nextId);
			}
			else
			{
				return new Message(targets[0], origin, sender, type, payload);
			}
		}

		private class ContractContext : IContractContext
		{
			private readonly ContractRegistry registry;

			public ContractContext(ContractRegistry registry, Address address)
			{
				this.Address = address;
				this.registry = registry;
			}

			public Address Address { get; }

			public void SendMessage(Address[] addresses, string type, IDictionary<string, object> payload)
			{
				this.registry.SendMessage(this.Address, addresses, type, payload);
			}

			public void ForwardMessage(ulong id)
			{
				this.registry.SendMessage(this.Address, id);
			}

			public Address CreateContract<T>(IDictionary<string, object> payload)
				where T : BaseContract, new()
			{
				return this.registry.CreateContract<T>(payload);
			}
		}
	}
}