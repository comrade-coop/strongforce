using System.Collections.Generic;
using StrongForce.Core.Tests.Mocks;
using Xunit;

namespace StrongForce.Core.Tests
{
	public class FavoriteNumberContractTests
	{
		private readonly BaseAddressFactory addressFactory = new RandomAddressFactory();

		[Fact]
		public void Receive_WhenPassedSetFavoriteNumberAction_SetsNumberCorrectly()
		{
			const int expectedNumber = 32;

			var (contract, receiver) = BaseContract.Create(typeof(FavoriteNumberContract), this.addressFactory.CreateAddress(), new Dictionary<string, object>() { { "User", null } }, default(ContractHandlers));

			var action = new Message(
				contract.Address,
				contract.Address,
				contract.Address,
				SetFavoriteNumberAction.Type,
				new Dictionary<string, object>()
				{
					{ SetFavoriteNumberAction.Number, expectedNumber },
				});

			receiver.Invoke(action);

			Assert.Equal(expectedNumber, ((FavoriteNumberContract)contract).Number);
		}
	}
}