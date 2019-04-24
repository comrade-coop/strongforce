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
		private readonly IAddressFactory AddressFactory;
		private ContractRegistryMock Registry;

		public PermittedContractTests()
		{
			this.AddressFactory = new RandomAddressFactory();
			this.Registry = new ContractRegistryMock();
		}

		[Fact]
		public void Permissions_WhenContractInitialized_ReturnTrue()
		{
			Address permissionManager = this.AddressFactory.Create();
			Address contractAddress = this.AddressFactory.Create();
			PermittedFavoriteNumberContract contract = new PermittedFavoriteNumberContract(contractAddress, Registry, permissionManager);

			var addPermission = new Permission(typeof(AddPermissionAction));
			var removePermission = new Permission(typeof(RemovePermissionAction));
			Assert.True(contract.CheckPermission(permissionManager, addPermission, contractAddress));
			Assert.True(contract.CheckPermission(permissionManager, removePermission, contractAddress));
		}

		[Fact]
		public void Receive_WhenPassedActionWithNoPermissions_ThrowsNoPermissionException()
		{
			Address address = this.AddressFactory.Create();
			Address permissionManager = this.AddressFactory.Create();
			Address contractAddress = this.AddressFactory.Create();
			Contract contract = new PermittedFavoriteNumberContract(contractAddress, Registry, permissionManager);

			this.Registry.RegisterContract(contract);

			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				new Permission(typeof(AddPermissionAction)),
				contractAddress);

			Assert.Throws<NoPermissionException>(() => this.Registry.HandleSendAction(addPermissionAction, contractAddress));
		}

		[Fact]
		public void Receive_WhenPassedSupportedActionWithPermissions_ReturnsTrue()
		{
			Address permissionManager = this.AddressFactory.Create();
			Address contractAddress = this.AddressFactory.Create();
			PermittedFavoriteNumberContract contract = new PermittedFavoriteNumberContract(contractAddress, Registry, permissionManager);
			this.Registry.RegisterContract(contract);
			var numberPermission = new Permission(typeof(SetFavoriteNumberAction));
			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				numberPermission,
				permissionManager);

			Assert.True(this.Registry.HandleSendAction(addPermissionAction, permissionManager));
			Assert.True(contract.CheckPermission(permissionManager, numberPermission, contractAddress));
		}

		[Fact]
		public void Receive_WhenRemovePermitedAction_ReturnsTrue()
		{
			Address permissionManager = this.AddressFactory.Create();
			Address contractAddress = this.AddressFactory.Create();
			PermittedFavoriteNumberContract contract = new PermittedFavoriteNumberContract(contractAddress, Registry, permissionManager);
			this.Registry.RegisterContract(contract);
			var numberPermission = new Permission(typeof(SetFavoriteNumberAction));
			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				numberPermission,
				permissionManager);

			var removeAddPermissionAction = new RemovePermissionAction(
				string.Empty,
				contractAddress,
				numberPermission,
				permissionManager,
				contractAddress);

			Assert.True(this.Registry.HandleSendAction(addPermissionAction, permissionManager));
			Assert.True(contract.CheckPermission(permissionManager, numberPermission, contractAddress));
			Assert.True(this.Registry.HandleSendAction(removeAddPermissionAction, permissionManager));
			Assert.False(contract.CheckPermission(permissionManager, numberPermission, contractAddress));
		}

		[Fact]
		public void Receive_WhenPassedUnsupportedActionWithPermissions_ReturnsFalse()
		{
			Address permissionManager = this.AddressFactory.Create();
			Address contractAddress = this.AddressFactory.Create();
			Contract contract = new PermittedFavoriteNumberContract(contractAddress, Registry, permissionManager);
			this.Registry.RegisterContract(contract);

			var addPermissionAction = new Action(
				string.Empty,
				contractAddress);

			Assert.Throws<NoPermissionException>(
				() => this.Registry.HandleSendAction(addPermissionAction, permissionManager));
		}
	}
}