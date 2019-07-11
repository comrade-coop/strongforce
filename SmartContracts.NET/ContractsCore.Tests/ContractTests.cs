using System;
using ContractsCore.Contracts;
using ContractsCore.Tests.Mocks;
using Xunit;
using Action = ContractsCore.Actions.Action;

namespace ContractsCore.Tests
{
	public class ContractTests
	{
		private readonly IAddressFactory addressFactory = new RandomAddressFactory();

		[Fact]
		public void Contract_WhenCreated_IsInitialisedWithSpecifiedAddress()
		{
			Address address = this.addressFactory.Create();
			Contract contract = new FavoriteNumberContract(address);
			Assert.Equal(address, contract.Address);
		}

		// [Fact]
		// public void Contract_WhenGetStateIsInvoked_ReturnsNonNullObject()
		// {
		//   Address address = this.addressFactory.Create();
		//   Contract contract = new FavoriteNumberContract(address);
		//   Assert.NotNull(contract.GetState());
		// }

		[Fact]
		public void Receive_WhenPassedNull_ThrowsArgumentNullException()
		{
			Address contractAddress = this.addressFactory.Create();
			Contract contract = new FavoriteNumberContract(contractAddress);

			Assert.Throws<ArgumentNullException>(() => contract.Receive(null));
		}

		[Fact]
		public void Receive_WhenReceivedSupportedAction_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			Contract contract = new FavoriteNumberContract(address);
			var action = new SetFavoriteNumberAction(
				string.Empty,
				address,
				0);

			Assert.True(contract.Receive(action));
		}

		[Fact]
		public void Receive_WhenReceivedUnsupportedAction_ReturnsFalse()
		{
			Address address = this.addressFactory.Create();
			Contract contract = new FavoriteNumberContract(address);
			var action = new Action(
				string.Empty,
				address);

			Assert.False(contract.Receive(action));
		}
	}
}