using System;
using System.Collections.Generic;
using System.Linq;
using StrongForce.Core;
using StrongForce.Core.Extensions;
using StrongForce.Core.Permissions;

namespace StrongForce.Core.Tests.Mocks
{
	public class TestRegistry
	{
		public TestRegistry(IAddressFactory addressFactory)
		{
			this.AddressFactory = addressFactory;
			this.Facade = new InMemoryIntegrationFacade();
			this.Registry = new ContractRegistry(this.Facade, this.AddressFactory);
		}

		public TestRegistry()
			: this(new RandomAddressFactory())
		{
		}

		public ContractRegistry Registry { get; set; }

		public InMemoryIntegrationFacade Facade { get; set; }

		public IAddressFactory AddressFactory { get; set; }

		public Address CreateContract<T>(IDictionary<string, object> payload = null)
		{
			var address = this.AddressFactory.Create();
			this.Facade.CreateContract(typeof(T), new FakeContractContext(address), payload ?? new Dictionary<string, object>());
			return address;
		}

		public void SendMessage(Address from, Address target, string type, IDictionary<string, object> payload)
		{
			this.SendMessage(from, new Address[] { target }, type, payload);
		}

		public void SendMessage(Address from, Address[] targets, string type, IDictionary<string, object> payload)
		{
			this.Facade.SendMessage(from, targets, type, payload);
		}

		public Contract GetContract(Address address)
		{
			return (Contract)this.Facade.LoadContract(address);
		}

		public T GetContract<T>(Address address)
			where T : BaseContract
		{
			return (T)this.Facade.LoadContract(address);
		}
	}
}