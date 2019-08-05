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

			Assert.Throws<ArgumentNullException>(() => contract.Receive(new ActionContext(Address.Null), null));
		}

		[Fact]
		public void Receive_WhenReceivedSupportedAction_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			Contract contract = new FavoriteNumberContract(address, Address.Null);
			var action = new SetFavoriteNumberAction(address, 0);

			Assert.True(contract.Receive(new ActionContext(Address.Null), action));
		}

		[Fact]
		public void Receive_WhenReceivedUnsupportedAction_ReturnsFalse()
		{
			Address address = this.addressFactory.Create();
			Contract contract = new FavoriteNumberContract(address, Address.Null);
			var action = new Action(address);

			contract.Acl.AddPermission(
				Address.Null,
				new Permission(typeof(Action)),
				address);

			Assert.False(contract.Receive(new ActionContext(Address.Null), action));
		}
	}
}