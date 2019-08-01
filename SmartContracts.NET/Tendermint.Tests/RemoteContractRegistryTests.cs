using System.Collections.Generic;
using System.Linq;
using StrongForce.Core;
using Tendermint.Tests.Mocks;
using Xunit;

namespace Tendermint.Tests
{
	public class RemoteContractRegistryTests
	{
		[Fact]
		public void Calls_And_Caches_Contract_Getter()
		{
			var count = 0;
			var contractCount = 10;
			var registry = new RemoteContractRegistry(address =>
			{
				count++;
				return new DummyContract(address);
			});

			// Repeat a few times to make sure it actually caches Contracts
			for (var r = 0; r < 3; r++)
			{
				for (var i = 0; i < contractCount; i++)
				{
					registry.GetContract(new Address(new byte[] { (byte)i }));
				}
			}

			Assert.Equal(contractCount, count);
		}

		[Fact]
		public void Calls_Contract_Getter_When_Messaging()
		{
			var count = 0;
			var messageCount = 10;
			var registry = new RemoteContractRegistry(address =>
			{
				count++;
				return new DummyContract(address, Address.Null);
			});

			for (var r = 0; r < 3; r++)
			{
				for (var i = 0; i < messageCount; i++)
				{
					var addressA = new Address(new byte[] { 0, (byte)i });
					var addressB = new Address(new byte[] { 1, (byte)i });
					registry.HandleAction(Address.Null, new DummyAction(addressA, new DummyAction(addressB)));
				}
			}

			Assert.Equal(messageCount * 2, count);
		}

		[Fact]
		public void Lists_Used_Contracts()
		{
			var expectedAddresses = new HashSet<Address>();
			var messageCount = 10;
			var registry = new RemoteContractRegistry(address =>
			{
				return new DummyContract(address, Address.Null);
			});

			for (var r = 0; r < 3; r++)
			{
				for (var i = 0; i < messageCount; i++)
				{
					var addressA = new Address(new byte[] { 0, (byte)i });
					var addressB = new Address(new byte[] { 1, (byte)i });
					expectedAddresses.Add(addressA);
					expectedAddresses.Add(addressB);
					registry.HandleAction(Address.Null, new DummyAction(addressA, new DummyAction(addressB)));
				}
			}

			Assert.True(expectedAddresses.SetEquals(registry.GetUsedContracts().Select(x => x.Address)));
		}
	}
}