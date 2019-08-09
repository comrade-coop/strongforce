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
			var contract = new FavoriteNumberContract(null);
			contract.Address = this.addressFactory.Create();

			var action = new SetFavoriteNumberAction(contract.Address, 0);
			Assert.True(contract.Receive(new ActionContext(contract.Address), action));
		}

		[Fact]
		public void Receive_WhenPassedSetFavoriteNumberAction_SetsNumberCorrectly()
		{
			const int expectedNumber = 32;

			var contract = new FavoriteNumberContract(null);
			contract.Address = this.addressFactory.Create();

			var action = new SetFavoriteNumberAction(contract.Address, expectedNumber);
			contract.Receive(new ActionContext(contract.Address), action);

			Assert.Equal(expectedNumber, contract.Number);
		}
	}
}