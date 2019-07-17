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
			this.Registry = new ContractRegistryMock(this.AddressFactory);
		}

		[Fact]
		public void Permissions_WhenContractInitialized_ReturnTrue()
		{
			Address permissionManager = this.AddressFactory.Create();
			Address contractAddress = this.AddressFactory.Create();
			PermittedFavoriteNumberContract contract =
				new PermittedFavoriteNumberContract(contractAddress, Registry, permissionManager);

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

			Assert.Throws<NoPermissionException>(() =>
				this.Registry.HandleSendAction(addPermissionAction, contractAddress));
		}

		[Fact]
		public void Receive_WhenPassedSupportedActionWithPermissions_ReturnsTrue()
		{
			Address permissionManager = this.AddressFactory.Create();
			Address contractAddress = this.AddressFactory.Create();
			PermittedFavoriteNumberContract contract =
				new PermittedFavoriteNumberContract(contractAddress, Registry, permissionManager);
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
			PermittedFavoriteNumberContract contract =
				new PermittedFavoriteNumberContract(contractAddress, Registry, permissionManager);
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

		[Fact]
		public void AddPermission_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.AddressFactory.Create();
			Address contractAddress = this.AddressFactory.Create();
			PermittedFavoriteNumberContract contract =
				new PermittedFavoriteNumberContract(contractAddress, Registry, permissionManager);
			this.Registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				permission,
				permissionManager);

			this.Registry.HandleSendAction(addPermissionAction, permissionManager);

			AnyWildCard anyWildCard = new AnyWildCard();
			addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				permission,
				anyWildCard);
			this.Registry.HandleSendAction(addPermissionAction, permissionManager);
			Assert.True(contract.CheckPermission(this.AddressFactory.Create(), permission, contractAddress));
		}

		[Fact]
		public void AddExecutorPermission_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.AddressFactory.Create();
			Address contractAddress = this.AddressFactory.Create();
			PermittedFavoriteNumberContract contract =
				new PermittedFavoriteNumberContract(contractAddress, Registry, permissionManager);
			this.Registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				permission,
				permissionManager);

			this.Registry.HandleSendAction(addPermissionAction, permissionManager);

			AnyWildCard anyWildCard = new AnyWildCard();
			addPermissionAction = new AddPermissionExecutorAction(
				string.Empty,
				contractAddress,
				permission,
				anyWildCard,
				contractAddress);

			this.Registry.HandleSendAction(addPermissionAction, permissionManager);

			Assert.True(contract.CheckPermission(this.AddressFactory.Create(), permission, contractAddress));
		}

		[Fact]
		public void AddPermissionNextAddress_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.AddressFactory.Create();
			Address contractAddress = this.AddressFactory.Create();
			PermittedFavoriteNumberContract contract =
				new PermittedFavoriteNumberContract(contractAddress, Registry, permissionManager);
			this.Registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				permission,
				permissionManager);

			this.Registry.HandleSendAction(addPermissionAction, permissionManager);
			Address nextAddress = this.AddressFactory.Create();

			Assert.True(contract.CheckPermission(permissionManager, permission, contractAddress));

			addPermissionAction = new AddPermissionNextAddressAction(
				string.Empty,
				contractAddress,
				permission,
				permissionManager,
				nextAddress);

			this.Registry.HandleSendAction(addPermissionAction, permissionManager);

			Assert.True(contract.CheckPermission(permissionManager, permission, nextAddress));
		}

		[Fact]
		public void RemovePermissionExecutor_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.AddressFactory.Create();
			Address contractAddress = this.AddressFactory.Create();
			PermittedFavoriteNumberContract contract =
				new PermittedFavoriteNumberContract(contractAddress, Registry, permissionManager);
			this.Registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			Address permitedAddress = this.AddressFactory.Create();
			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				permission,
				new AddressWildCard() {permissionManager, permitedAddress});

			this.Registry.HandleSendAction(addPermissionAction, permissionManager);

			AnyWildCard anyWildCard = new AnyWildCard();
			var removePermissionAction = new RemovePermissionExecutorAction(
				string.Empty,
				contractAddress,
				permission,
				permitedAddress,
				contractAddress);
			this.Registry.HandleSendAction(removePermissionAction, permissionManager);
			Assert.False(contract.CheckPermission(permitedAddress, permission, contractAddress));
		}

		[Fact]
		public void RemovePermissionNextAddress_WhenRemovingSingleAddress_ReturnsFalse()
		{
			Address permissionManager = this.AddressFactory.Create();
			Address contractAddress = this.AddressFactory.Create();
			PermittedFavoriteNumberContract contract =
				new PermittedFavoriteNumberContract(contractAddress, Registry, permissionManager);
			this.Registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			Address nextAddress1 = this.AddressFactory.Create();
			Address nextAddress2 = this.AddressFactory.Create();
			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				permission,
				permissionManager,
				new AddressWildCard() {nextAddress1, nextAddress2});

			this.Registry.HandleSendAction(addPermissionAction, permissionManager);

			AnyWildCard anyWildCard = new AnyWildCard();
			var removePermissionAction = new RemovePermissionNextAddressAction(
				string.Empty,
				contractAddress,
				permission,
				permissionManager,
				nextAddress1);
			this.Registry.HandleSendAction(removePermissionAction, permissionManager);
			Assert.False(contract.CheckPermission(permissionManager, permission, nextAddress1));
			Assert.True(contract.CheckPermission(permissionManager, permission, nextAddress2));
		}

		[Fact]
		public void RemovePermissionNextAddress_WhenRemovingWildCard_ReturnsTrue()
		{
			Address permissionManager = this.AddressFactory.Create();
			Address contractAddress = this.AddressFactory.Create();
			PermittedFavoriteNumberContract contract =
				new PermittedFavoriteNumberContract(contractAddress, Registry, permissionManager);
			this.Registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			Address address = this.AddressFactory.Create();
			var anyWildCard = new AnyWildCard();
			var set = new WildCardSet();
			set.AddAddress(address);
			set.AddWildCard(anyWildCard);
			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				permission,
				permissionManager,
				set);

			this.Registry.HandleSendAction(addPermissionAction, permissionManager);

			Assert.True(contract.CheckPermission(permissionManager, permission, this.AddressFactory.Create()));

			var removePermissionAction = new RemovePermissionNextAddressAction(
				string.Empty,
				contractAddress,
				permission,
				permissionManager,
				anyWildCard);
			this.Registry.HandleSendAction(removePermissionAction, permissionManager);
			Assert.False(contract.CheckPermission(permissionManager, permission, this.AddressFactory.Create()));
			Assert.True(contract.CheckPermission(permissionManager, permission, address));
		}
	}
}