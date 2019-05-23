using ContractsCore.Actions;
using ContractsCore.Contracts;
using ContractsCore.Exceptions;
using ContractsCore.Permissions;
using ContractsCore.Permissions.WildCard;
using ContractsCore.Tests.Mocks;
using Xunit;
using Action = ContractsCore.Actions.Action;

namespace ContractsCore.Tests
{
	public class PermittedContractTests
	{
		private readonly IAddressFactory addressFactory;
		private readonly ContractRegistryMock registry;

		public PermittedContractTests()
		{
			this.addressFactory = new RandomAddressFactory();
			this.registry = new ContractRegistryMock();
		}

		[Fact]
		public void Permissions_WhenContractInitialized_ReturnTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			var contract = new PermittedFavoriteNumberContract(permissionManager);

			var addPermission = new Permission(typeof(AddPermissionAction));
			var removePermission = new Permission(typeof(RemovePermissionAction));
			Assert.True(contract.HasPermission(permissionManager, addPermission));
			Assert.True(contract.HasPermission(permissionManager, removePermission));
		}

		[Fact]
		public void Receive_WhenPassedActionWithNoPermissions_ThrowsNoPermissionException()
		{
			Address permissionManager = this.addressFactory.Create();
			Contract contract = new PermittedFavoriteNumberContract(permissionManager);

			var addPermissionAction = new AddPermissionAction(
				contract.Address,
				new Permission(typeof(AddPermissionAction)),
				contract.Address);

			Assert.Throws<NoPermissionException>(() =>
				contract.Receive(addPermissionAction));
		}

		[Fact]
		public void Receive_WhenPassedSupportedActionWithPermissions_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();

			var contract = new PermittedFavoriteNumberContract(permissionManager);
			var numberPermission = new Permission(typeof(SetFavoriteNumberAction));
			var addPermissionAction = new AddPermissionAction(
				contract.Address,
				numberPermission,
				permissionManager);

			Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));
			Assert.True(contract.HasPermission(permissionManager, numberPermission));
		}

		[Fact]
		public void Receive_WhenRemovePermitedAction_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			var contract =
				new PermittedFavoriteNumberContract(permissionManager);
			var numberPermission = new Permission(typeof(SetFavoriteNumberAction));
			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				numberPermission,
				permissionManager);

			var removeAddPermissionAction = new RemovePermissionAction(
				contractAddress,
				numberPermission,
				permissionManager,
				contractAddress);

			Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));
			Assert.True(contract.HasPermission(permissionManager, numberPermission));
			Assert.True(this.registry.HandleSendAction(removeAddPermissionAction, permissionManager));
			Assert.False(contract.HasPermission(permissionManager, numberPermission));
		}

		[Fact]
		public void Receive_WhenPassedUnsupportedActionWithPermissions_ReturnsFalse()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			Contract contract = new PermittedFavoriteNumberContract(permissionManager);

			var addPermissionAction = new Action(
				contractAddress);

			Assert.Throws<NoPermissionException>(
				() => this.registry.HandleSendAction(addPermissionAction, permissionManager));
		}

		[Fact]
		public void AddPermission_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			var contract = new PermittedFavoriteNumberContract(permissionManager);
			var permission = new Permission(typeof(Action));
			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				permissionManager);

			this.registry.HandleSendAction(addPermissionAction, permissionManager);

			AnyWildCard anyWildCard = new AnyWildCard();
			addPermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				anyWildCard);
			this.registry.HandleSendAction(addPermissionAction, permissionManager);
			Assert.True(contract.HasPermission(this.addressFactory.Create(), permission));
		}

		[Fact]
		public void AddExecutorPermission_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			var contract = new PermittedFavoriteNumberContract(permissionManager);
			var permission = new Permission(typeof(Action));
			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				permissionManager);

			this.registry.HandleSendAction(addPermissionAction, permissionManager);

			AnyWildCard anyWildCard = new AnyWildCard();
			addPermissionAction = new AddPermissionExecutorAction(
				contractAddress,
				permission,
				anyWildCard,
				contractAddress);

			this.registry.HandleSendAction(addPermissionAction, permissionManager);

			Assert.True(contract.HasPermission(this.addressFactory.Create(), permission));
		}

		[Fact]
		public void AddPermissionNextAddress_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			PermittedFavoriteNumberContract contract =
				new PermittedFavoriteNumberContract(permissionManager);
			var permission = new Permission(typeof(Action));
			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				permissionManager);

			this.registry.HandleSendAction(addPermissionAction, permissionManager);
			Address nextAddress = this.addressFactory.Create();

			Assert.True(contract.HasPermission(permissionManager, permission));

			addPermissionAction = new AddPermissionNextAddressAction(
				contractAddress,
				permission,
				permissionManager,
				nextAddress);

			this.registry.HandleSendAction(addPermissionAction, permissionManager);

			Assert.True(contract.HasPermission(permissionManager, permission));
		}

		[Fact]
		public void RemovePermissionExecutor_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			PermittedFavoriteNumberContract contract =
				new PermittedFavoriteNumberContract(permissionManager);

			var permission = new Permission(typeof(Action));
			Address permitedAddress = this.addressFactory.Create();
			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				new AddressWildCard() {permissionManager, permitedAddress});

			this.registry.HandleSendAction(addPermissionAction, permissionManager);

			AnyWildCard anyWildCard = new AnyWildCard();
			var removePermissionAction = new RemovePermissionExecutorAction(
				contractAddress,
				permission,
				permitedAddress,
				contractAddress);
			this.registry.HandleSendAction(removePermissionAction, permissionManager);
			Assert.False(contract.HasPermission(permitedAddress, permission));
		}

		[Fact]
		public void RemovePermissionNextAddress_WhenRemovingSingleAddress_ReturnsFalse()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			var contract =
				new PermittedFavoriteNumberContract(permissionManager);

			var permission = new Permission(typeof(Action));
			Address nextAddress1 = this.addressFactory.Create();
			Address nextAddress2 = this.addressFactory.Create();
			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				permissionManager,
				new AddressWildCard {nextAddress1, nextAddress2});

			this.registry.HandleSendAction(addPermissionAction, permissionManager);

			var anyWildCard = new AnyWildCard();
			var removePermissionAction = new RemovePermissionNextAddressAction(
				contractAddress,
				permission,
				permissionManager,
				nextAddress1);
			this.registry.HandleSendAction(removePermissionAction, permissionManager);
			Assert.False(contract.HasPermission(permissionManager, permission));
			Assert.True(contract.HasPermission(permissionManager, permission));
		}

		[Fact]
		public void RemovePermissionNextAddress_WhenRemovingWildCard_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			var contract =
				new PermittedFavoriteNumberContract(permissionManager);

			var permission = new Permission(typeof(Action));
			Address address = this.addressFactory.Create();
			var anyWildCard = new AnyWildCard();
			var set = new WildCardSet();
			set.AddAddress(address);
			set.AddWildCard(anyWildCard);
			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				permissionManager,
				set);

			this.registry.HandleSendAction(addPermissionAction, permissionManager);

			Assert.True(contract.HasPermission(permissionManager, permission));

			var removePermissionAction = new RemovePermissionNextAddressAction(
				contractAddress,
				permission,
				permissionManager,
				anyWildCard);
			this.registry.HandleSendAction(removePermissionAction, permissionManager);
			Assert.False(contract.HasPermission(permissionManager, permission));
			Assert.True(contract.HasPermission(permissionManager, permission));
		}
	}
}