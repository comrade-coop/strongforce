using System;
using System.Collections.Generic;
using System.Linq;
using StrongForce.Core.Kits;
using StrongForce.Core.Permissions;
using StrongForce.Core.Tests.Mocks;
using Xunit;

namespace StrongForce.Core.Tests
{
	public class KitTests
	{
		[Fact]
		public void Kit_WhenInstantiated_CreatesContracts()
		{
			var favoriteContractsCount = 2;

			var registry = new TestRegistry();

			Address address = registry.CreateContract<FavoriteNumberKitContract>(new Dictionary<string, object>()
			{
				{ "User", null },
				{ "FavoriteContractsCount", favoriteContractsCount },
			});

			registry.SendMessage(address, address, InstantiateKitAction.Type, new Dictionary<string, object>());

			Assert.Equal(favoriteContractsCount + 1, (int)((HashedAddressFactory)registry.RawAddressFactory).AddressNonce - 1);
		}

		[Fact]
		public void Kit_WhenInstantiatedTwice_Throws()
		{
			var registry = new TestRegistry();

			Address address = registry.CreateContract<FavoriteNumberKitContract>(new Dictionary<string, object>()
			{
				{ "User", null },
				{ "FavoriteContractsCount", 2 },
			});

			registry.SendMessage(address, address, InstantiateKitAction.Type, new Dictionary<string, object>());
			Assert.Throws<InvalidOperationException>(() => registry.SendMessage(address, address, InstantiateKitAction.Type, new Dictionary<string, object>()));
		}
	}
}