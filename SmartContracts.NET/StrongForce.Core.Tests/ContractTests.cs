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
		public void Receive_WhenPassedNull_ThrowsArgumentNullException()
		{
			Contract contract = new FavoriteNumberContract(null);
			contract.Address = this.addressFactory.Create();

			Assert.Throws<ArgumentNullException>(() => contract.Receive(new ActionContext(contract.Address), null));
		}

		[Fact]
		public void Receive_WhenReceivedSupportedAction_ReturnsTrue()
		{
			Contract contract = new FavoriteNumberContract(null);
			contract.Address = this.addressFactory.Create();
			var action = new SetFavoriteNumberAction(contract.Address, 0);

			Assert.True(contract.Receive(new ActionContext(contract.Address), action));
		}

		[Fact]
		public void Receive_WhenReceivedUnsupportedAction_ReturnsFalse()
		{
			Contract contract = new FavoriteNumberContract(null);
			contract.Address = this.addressFactory.Create();
			var action = new Action(contract.Address);

			contract.Acl.AddPermission(
				contract.Address,
				typeof(Action),
				contract.Address);

			Assert.False(contract.Receive(new ActionContext(contract.Address), action));
		}
	}
}