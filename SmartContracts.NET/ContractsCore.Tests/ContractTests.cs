using ContractsCore.Actions;
using ContractsCore.Tests.Mocks;
using Xunit;

namespace ContractsCore.Tests
{
	public class ContractTests
	{
		private readonly IAddressFactory addressFactory;

		public ContractTests()
		{
			this.addressFactory = new RandomAddressFactory();
		}

		[Fact]
		public void Contract_WhenCreated_IsInitialisedWithSpecifiedAddress()
		{
			Address address = this.addressFactory.Create();
			Contract contract = new FavoriteNumberContract(address);
			Assert.Equal(address, contract.Address);
		}

		[Fact]
		public void Contract_WhenGetStateIsInvoked_ReturnsNonNullObject()
		{
			Address address = this.addressFactory.Create();
			Contract contract = new FavoriteNumberContract(address);
			Assert.NotNull(contract.GetState());
		}

		[Fact]
		public void Receive_WhenReceivedSupportedAction_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			Contract contract = new FavoriteNumberContract(address);
			var action = new SetFavoriteNumberAction(
				string.Empty,
				address,
				address,
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
				address,
				address,
				address);

			Assert.False(contract.Receive(action));
		}
	}
}