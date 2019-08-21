using System.Collections.Generic;
using StrongForce.Core.Tests.Mocks;
using Xunit;

namespace StrongForce.Core.Tests
{
	public class FavoriteNumberContractTests
	{
		private readonly IAddressFactory addressFactory = new RandomAddressFactory();

		[Fact]
		public void Receive_WhenPassedSetFavoriteNumberAction_SetsNumberCorrectly()
		{
			const int expectedNumber = 32;

			var contract = new FavoriteNumberContract();
			contract.Configure(this.addressFactory.Create(), new Dictionary<string, object>(new Dictionary<string, object>()
			{
				{ "User", null },
			}));

			var action = new PayloadAction(
				contract.Address,
				contract.Address,
				contract.Address,
				SetFavoriteNumberAction.Type,
				new Dictionary<string, object>()
				{
					{ SetFavoriteNumberAction.Number, expectedNumber },
				});

			Assert.True(contract.Receive(action));

			Assert.Equal(expectedNumber, contract.Number);
		}
	}
}