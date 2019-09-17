using System;
using System.Collections.Generic;
using System.Linq;
using StrongForce.Core;
using StrongForce.Core.Extensions;
using StrongForce.Core.Permissions;

namespace StrongForce.Core.Tests.Mocks
{
	public class TestRegistryAddressFactory : BaseAddressFactory
	{
		public TestRegistryAddressFactory(InMemoryIntegrationFacade facade)
		{
			this.Facade = facade;
		}

		public InMemoryIntegrationFacade Facade { get; set; }

		public override Address CreateAddress()
		{
			var state = this.Facade.LoadRegistryState();
			var address = state.AddressFactory.CreateAddress();
			this.Facade.SaveRegistryState(state);
			this.Facade.DropCache();
			return address;
		}
	}
}