using System;
using System.Collections.Generic;
using StrongForce.Core.Permissions;
using StrongForce.Core.Tests.Mocks;
using Xunit;

namespace StrongForce.Core.Tests
{
	public class ContractTests
	{
		private readonly IAddressFactory addressFactory = new RandomAddressFactory();

		[Fact]
		public void Receive_WhenPassedNull_ThrowsArgumentNullException()
		{
			Contract contract = new FavoriteNumberContract(null);
			contract.Address = this.addressFactory.Create();

			Assert.Throws<ArgumentNullException>(() => contract.Receive(null));
		}

		[Fact]
		public void Receive_WhenReceivedSupportedAction_ReturnsTrue()
		{
			Contract contract = new FavoriteNumberContract(null);
			contract.Address = this.addressFactory.Create();

			Assert.True(contract.Receive(new PayloadAction(
				contract.Address,
				contract.Address,
				contract.Address,
				SetFavoriteNumberAction.Type,
				new Dictionary<string, object>()
				{
					{ SetFavoriteNumberAction.Number, 0 },
				})));
		}

		[Fact]
		public void Receive_WhenReceivedUnsupportedAction_ReturnsFalse()
		{
			Contract contract = new FavoriteNumberContract(null);
			contract.Address = this.addressFactory.Create();
			var actionType = "NotARealActionType";

			contract.Acl.AddPermission(
				contract.Address,
				actionType,
				contract.Address);

			Assert.False(contract.Receive(new PayloadAction(
				contract.Address,
				contract.Address,
				contract.Address,
				actionType,
				new Dictionary<string, object>())));
		}
	}
}