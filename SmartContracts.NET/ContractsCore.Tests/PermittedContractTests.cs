using ContractsCore.Actions;
using ContractsCore.Contracts;
using ContractsCore.Exceptions;
using ContractsCore.Permissions;
using ContractsCore.Tests.Mocks;
using Xunit;
using Action = ContractsCore.Actions.Action;

namespace ContractsCore.Tests
{
	public class PermittedContractTests
	{
		private readonly IAddressFactory addressFactory = new RandomAddressFactory();

		[Fact]
		public void Receive_WhenPassedActionWithNoPermissions_ThrowsNoPermissionException()
		{
			Address address = this.addressFactory.Create();
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			Contract contract = new PermittedFavoriteNumberContract(contractAddress, permissionManager);

			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				address,
				contractAddress,
				address,
				address,
				new Permission(typeof(AddPermissionAction)));

			Assert.Throws<NoPermissionException>(() => contract.Receive(addPermissionAction));
		}

		[Fact]
		public void Receive_WhenPassedSupportedActionWithPermissions_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			PermittedContract contract = new PermittedFavoriteNumberContract(contractAddress, permissionManager);

			var removePermission = new Permission(typeof(RemovePermissionAction));
			var addRemovePermissionAction = new AddPermissionAction(
				string.Empty,
				permissionManager,
				permissionManager,
				contractAddress,
				contractAddress,
				removePermission);

			Assert.True(contract.Receive(addRemovePermissionAction));
			Assert.True(contract.HasPermission(contractAddress, removePermission));

			var addPermission = new Permission(typeof(AddPermissionAction));
			var removeAddPermissionAction = new RemovePermissionAction(
				string.Empty,
				permissionManager,
				permissionManager,
				contractAddress,
				permissionManager,
				addPermission);

			Assert.True(contract.Receive(removeAddPermissionAction));
			Assert.False(contract.HasPermission(permissionManager, addPermission));
		}

		[Fact]
		public void Receive_WhenPassedUnsupportedActionWithPermissions_ReturnsFalse()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			Contract contract = new PermittedFavoriteNumberContract(contractAddress, permissionManager);

			var addAction = new AddPermissionAction(
				string.Empty,
				permissionManager,
				permissionManager,
				contractAddress,
				permissionManager,
				new Permission(typeof(Action)));

			contract.Receive(addAction);

			var addPermissionAction = new Action(
				string.Empty,
				permissionManager,
				permissionManager,
				contractAddress);

			Assert.False(contract.Receive(addPermissionAction));
		}
	}
}