using System;
using System.Collections.Generic;

namespace StrongForce.Core
{
	public class InMemoryIntegration : IContractRegistryContext
	{
		private Dictionary<Address, (Type, IDictionary<string, object>)> storedContracts = new Dictionary<Address, (Type, IDictionary<string, object>)>();

		private IDictionary<string, object> storedRegistryState = new Dictionary<string, object>();

		public InMemoryIntegration()
		{
			this.AddressFactory = new ProxyAddressFactory(this);
		}

		public ProxyAddressFactory AddressFactory { get; }

		public BaseContract LoadContract(Address address)
		{
			var (type, state) = this.storedContracts[address];
			return (BaseContract)StatefulObject.Create(type, state, true);
		}

		public void SaveContract(BaseContract contract)
		{
			var state = contract.GetState();
			var type = contract.GetType();
			this.storedContracts[contract.Address] = (type, state);
		}

		public (ExposedContractRegistry, Action<Address, Address[], string, IDictionary<string, object>>) LoadRegistry()
		{
			var registry = StatefulObject.Create<ExposedContractRegistry>(this.storedRegistryState);
			var receiveMessage = registry.SetContext(this);

			return (registry, receiveMessage);
		}

		public void SaveRegistry(ExposedContractRegistry registry)
		{
			this.storedRegistryState = registry.GetState();
		}

		public void SendMessage(Address from, Address[] targets, string type, IDictionary<string, object> payload)
		{
			try
			{
				var (registry, receiveMessage) = this.LoadRegistry();

				receiveMessage.Invoke(from, targets, type, payload);

				this.SaveRegistry(registry);
			}
			catch (Exception)
			{
				throw;
			}
		}

		public void SendMessage(Address from, Address target, string type, IDictionary<string, object> payload)
		{
			this.SendMessage(from, new Address[] { target }, type, payload);
		}

		public Address CreateContract(Type type, IDictionary<string, object> payload = null)
		{
			var address = this.AddressFactory.CreateAddress();
			var contract = (BaseContract)StatefulObject.Create(type, payload);

			contract.RegisterWithRegistry(new FakeContractContext(address));
			this.storedContracts[contract.Address] = (type, contract.GetState());

			return address;
		}

		public Address CreateContract<T>(IDictionary<string, object> payload = null)
		{
			return this.CreateContract(typeof(T), payload ?? new Dictionary<string, object>());
		}

		public T GetContract<T>(Address address)
			where T : BaseContract
		{
			var (type, state) = this.storedContracts[address];
			return (T)StatefulObject.Create(type, state, true);
		}

		public Contract GetContract(Address address)
		{
			return this.GetContract<Contract>(address);
		}

		public class FakeContractContext : IContractContext
		{
			public FakeContractContext(Address address)
			{
				this.Address = address;
			}

			public Address Address { get; }

			public void SendMessage(Address[] addresses, string type, IDictionary<string, object> payload)
			{
				throw new NotSupportedException();
			}

			public void ForwardMessage(ulong id)
			{
				throw new NotSupportedException();
			}

			public Address CreateContract<T>(IDictionary<string, object> payload)
				where T : BaseContract, new()
			{
				throw new NotSupportedException();
			}
		}

		public class ProxyAddressFactory : BaseAddressFactory
		{
			public ProxyAddressFactory()
			{
				this.Facade = null;
			}

			public ProxyAddressFactory(InMemoryIntegration facade)
			{
				this.Facade = facade;
			}

			public InMemoryIntegration Facade { get; set; }

			public BaseAddressFactory Raw { get => this.Facade.LoadRegistry().Item1.AddressFactory; }

			public override Address CreateAddress()
			{
				var (registry, receiveMessage) = this.Facade.LoadRegistry();

				var address = registry.AddressFactory.CreateAddress();

				this.Facade.SaveRegistry(registry);

				return address;
			}
		}

		public class ExposedContractRegistry : ContractRegistry
		{
			public new BaseAddressFactory AddressFactory { get => base.AddressFactory; }
		}
	}
}