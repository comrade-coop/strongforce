using StrongForce.Core.Tests.Mocks;
using Xunit;

namespace StrongForce.Core.Tests
{
	public class FavoriteNumberContractTests
	{
		private readonly IAddressFactory addressFactory = new RandomAddressFactory();

		[Fact]
		public void Receive_WhenPassedSetFavoriteNumberAction_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			var contract = new FavoriteNumberContract(address, null);
			var action = new SetFavoriteNumberAction(address, 0);
			Assert.True(contract.Receive(new ActionContext(address), action));
		}

		[Fact]
		public void Receive_WhenPassedSetFavoriteNumberAction_SetsNumberCorrectly()
		{
			const int expectedNumber = 32;
			Address address = this.addressFactory.Create();
			var contract = new FavoriteNumberContract(address, null);
			var action = new SetFavoriteNumberAction(address, expectedNumber);
			contract.Receive(new ActionContext(address), action);

			Assert.Equal(expectedNumber, contract.Number);
		}
	}
}