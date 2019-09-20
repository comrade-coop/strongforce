using System;
using System.Collections.Generic;
using System.Linq;
using StrongForce.Core.Exceptions;
using StrongForce.Core.Serialization;

namespace StrongForce.Core
{
	public class ContractRegistry
	{
		public ContractRegistry(IIntegrationFacade facade)
		{
			this.IntegrationFacade = facade;

			this.IntegrationFacade.ReceiveMessage += (from, targets, type, payload) =>
			{
				this.Errored = false;

				if (this.State == null)
				{
					this.State = this.IntegrationFacade.LoadRegistryState();
				}

				this.SendMessage(from, targets, type, payload);

				foreach (var contract in this.CachedContracts)
				{
					this.IntegrationFacade.SaveContract(contract.Value.Item1);
				}

				this.IntegrationFacade.SaveRegistryState(this.State);
			};
			this.IntegrationFacade.DropCaches += this.DropCaches;
		}

		public ContractRegistry()
			: this(new InMemoryIntegrationFacade())
		{
		}

		protected bool Errored { get; set; }

		protected IIntegrationFacade IntegrationFacade { get; set; }

		protected ContractRegistryState State { get; set; } = null;

		protected IDictionary<Address, (BaseContract, Action<Message>)> CachedContracts { get; set; }
			= new Dictionary<Address, (BaseContract, Action<Message>)>();

		protected void DropCaches()
		{
			this.CachedContracts = new Dictionary<Address, (BaseContract, Action<Message>)>();
			this.State = null;
		}

		protected void SendMessage(Address sender, Address[] targets, string type, IDictionary<string, object> payload)
		{
			var message = this.CreateMessage(sender, sender, targets, type, payload);

			this.DispatchMessage(message);
		}

		protected void SendMessage(Address sender, ulong id)
		{
			var message = this.State.ForwardedMessages[id];

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

			var address = this.State.AddressFactory.CreateAddress();

			var contract = StatefulObject.Create<T>(payload);

			var receiver = contract.RegisterWithRegistry(new ContractContext(this, address));

			this.CachedContracts.Add(address, (contract, receiver));
			this.IntegrationFacade.SaveContract(contract);

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
				var contract = this.IntegrationFacade.LoadContract(address);

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
				var nextId = this.State.MessageNonce;
				this.State.MessageNonce++;

				var nextTargets = targets.Skip(1).ToArray();

				this.State.ForwardedMessages.Add(nextId, this.CreateMessage(origin, targets[0], nextTargets, type, payload));

				return new ForwardMessage(
					targets[0],
					origin,
					sender,
					type,
					payload,
					nextTargets,
					nextId);
			}
			else
			{
				return new Message(
					targets[0],
					origin,
					sender,
					type,
					payload);
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