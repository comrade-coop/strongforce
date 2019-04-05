using ContractsCore.Actions;
using ContractsCore.Exceptions;
using ContractsCore.Permissions;
using ContractsCore.Tests.Mocks;
using Xunit;

namespace ContractsCore.Tests
{
	public class PermittedFavoriteNumberContractTests
	{
		private readonly IAddressFactory addressFactory = new RandomAddressFactory();

		[Fact]
		public void Receive_WhenPassedSetFavoriteNumberActionWithGrantedPermissions_SetsNumberCorrectly()
		{
			Address address = this.addressFactory.Create();
			Address permissionManager = this.addressFactory.Create();
			var contract = new PermittedFavoriteNumberContract(address, permissionManager);

			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				permissionManager,
				permissionManager,
				address,
				address,
				new Permission(typeof(SetFavoriteNumberAction)));

			contract.Receive(addPermissionAction);

			const int expectedNumber = 32;
			var setNumberAction = new SetFavoriteNumberAction(
				string.Empty,
				address,
				address,
				address,
				expectedNumber);

			Assert.True(contract.Receive(setNumberAction));
			Assert.Equal(expectedNumber, contract.Number);
		}

		[Fact]
		public void Receive_WhenPassedSetFavoriteNumberActionWithoutPermissions_ThrowsNoPermissionException()
		{
			Address address = this.addressFactory.Create();
			var contract = new PermittedFavoriteNumberContract(address, address);

			var setNumberAction = new SetFavoriteNumberAction(
				string.Empty,
				address,
				address,
				address,
				0);

			Assert.Throws<NoPermissionException>(() => contract.Receive(setNumberAction));
		}
	}
}