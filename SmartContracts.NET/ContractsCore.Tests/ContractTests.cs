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
		public void Contract_WhenGetStateIsInvoked_ReturnsNonNullObject()
		{
			Contract contract = new FavoriteNumberContract();
			Assert.NotNull(contract.GetState());
		}

		[Fact]
		public void Receive_WhenPassedNull_ThrowsArgumentNullException()
		{
			Contract contract = new FavoriteNumberContract();

			Assert.Throws<ArgumentNullException>(() => contract.Receive(null));
		}

		[Fact]
		public void Receive_WhenReceivedSupportedAction_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			Contract contract = new FavoriteNumberContract();
			var action = new SetFavoriteNumberAction(
				address,
				0);

			Assert.True(contract.Receive(action));
		}

		[Fact]
		public void Receive_WhenReceivedUnsupportedAction_ReturnsFalse()
		{
			Address address = this.addressFactory.Create();
			Contract contract = new FavoriteNumberContract();
			var action = new Action(address);

			Assert.False(contract.Receive(action));
		}
	}
}