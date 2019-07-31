using System;
using StrongForce.Core.Permissions;
using StrongForce.Core.Tests.Mocks;
using Xunit;

namespace StrongForce.Core.Tests
{
	public class ContractTests
	{
		private readonly IAddressFactory addressFactory = new RandomAddressFactory();

		[Fact]
		public void Contract_WhenCreated_IsInitialisedWithSpecifiedAddress()
		{
			Address address = this.addressFactory.Create();
			Contract contract = new FavoriteNumberContract(address, Address.Null);
			Assert.Equal(address, contract.Address);
		}

		[Fact]
		public void Receive_WhenPassedNull_ThrowsArgumentNullException()
		{
			Address contractAddress = this.addressFactory.Create();
			Contract contract = new FavoriteNumberContract(contractAddress, Address.Null);

			Assert.Throws<ArgumentNullException>(() => contract.Receive(null));
		}

		[Fact]
		public void Receive_WhenReceivedSupportedAction_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			Contract contract = new FavoriteNumberContract(address, Address.Null);
			var action = new SetFavoriteNumberAction(address, 0).ConfigureSenderAndOrigin(Address.Null);

			Assert.True(contract.Receive(action));
		}

		[Fact]
		public void Receive_WhenReceivedUnsupportedAction_ReturnsFalse()
		{
			Address address = this.addressFactory.Create();
			Contract contract = new FavoriteNumberContract(address, Address.Null);
			var action = new Action(address).ConfigureSenderAndOrigin(Address.Null);

			contract.Acl.AddPermission(
				Address.Null,
				new Permission(typeof(Action)),
				address);

			Assert.False(contract.Receive(action));
		}
	}
}