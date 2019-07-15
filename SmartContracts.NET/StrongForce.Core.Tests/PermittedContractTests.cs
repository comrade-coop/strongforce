using StrongForce.Core.Exceptions;
using StrongForce.Core.Permissions;
using StrongForce.Core.Tests.Mocks;
using Xunit;

namespace StrongForce.Core.Tests
{
	public class PermittedContractTests
	{
		private readonly IAddressFactory addressFactory;
		private ContractRegistryMock registry;

		public PermittedContractTests()
		{
			this.addressFactory = new RandomAddressFactory();
			this.registry = new ContractRegistryMock();
		}

		[Fact]
		public void Permissions_WhenContractInitialized_ReturnTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			PermittedFavoriteNumberContract contract = new PermittedFavoriteNumberContract(contractAddress, permissionManager);

			var addPermission = new Permission(typeof(AddPermissionAction));
			var removePermission = new Permission(typeof(RemovePermissionAction));
			Assert.True(contract.CheckPermission(permissionManager, addPermission, contractAddress));
			Assert.True(contract.CheckPermission(permissionManager, removePermission, contractAddress));
		}

		[Fact]
		public void Receive_WhenPassedActionWithNoPermissions_ThrowsNoPermissionException()
		{
			Address address = this.addressFactory.Create();
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			Contract contract = new PermittedFavoriteNumberContract(contractAddress, permissionManager);

			this.registry.RegisterContract(contract);

			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				new Permission(typeof(AddPermissionAction)),
				contractAddress);

			Assert.Throws<NoPermissionException>(() => this.registry.HandleSendAction(addPermissionAction, contractAddress));
		}

		[Fact]
		public void Receive_WhenPassedSupportedActionWithPermissions_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			PermittedFavoriteNumberContract contract = new PermittedFavoriteNumberContract(contractAddress, permissionManager);
			this.registry.RegisterContract(contract);
			var numberPermission = new Permission(typeof(SetFavoriteNumberAction));
			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				numberPermission,
				permissionManager);

			Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));
			Assert.True(contract.CheckPermission(permissionManager, numberPermission, contractAddress));
		}

		[Fact]
		public void Receive_WhenRemovePermitedAction_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			PermittedFavoriteNumberContract contract = new PermittedFavoriteNumberContract(contractAddress, permissionManager);
			this.registry.RegisterContract(contract);
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
				new AddressWildCard() { permissionManager },
				new AddressWildCard() { contractAddress });

			Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));
			Assert.True(contract.CheckPermission(permissionManager, numberPermission, contractAddress));
			Assert.True(this.registry.HandleSendAction(removeAddPermissionAction, permissionManager));
			Assert.False(contract.CheckPermission(permissionManager, numberPermission, contractAddress));
		}

		[Fact]
		public void Receive_WhenPassedUnsupportedActionWithPermissions_ReturnsFalse()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			Contract contract = new PermittedFavoriteNumberContract(contractAddress, permissionManager);
			this.registry.RegisterContract(contract);

			var addPermissionAction = new Action(
				string.Empty,
				contractAddress);

			Assert.Throws<NoPermissionException>(
				() => this.registry.HandleSendAction(addPermissionAction, permissionManager));
		}

		[Fact]
		public void AddPermission_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			PermittedFavoriteNumberContract contract = new PermittedFavoriteNumberContract(contractAddress, permissionManager);
			this.registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				permission,
				permissionManager);

			this.registry.HandleSendAction(addPermissionAction, permissionManager);

			AnyWildCard anyWildCard = new AnyWildCard();
			addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				permission,
				anyWildCard);
			this.registry.HandleSendAction(addPermissionAction, permissionManager);
			Assert.True(contract.CheckPermission(this.addressFactory.Create(), permission, contractAddress));
		}

		[Fact]
		public void UpdatePermission_WhenPermissionExists_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			PermittedFavoriteNumberContract contract = new PermittedFavoriteNumberContract(contractAddress, permissionManager);
			this.registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				permission,
				permissionManager);

			this.registry.HandleSendAction(addPermissionAction, permissionManager);

			var updatePermissionAction = new UpdatePermissionAction(
				string.Empty,
				contractAddress,
				new AddressWildCard() { permissionManager },
				new AddressWildCard() { contractAddress },
				permission,
				new AddressWildCard() { permissionManager, address },
				new AddressWildCard() { contractAddress });

			this.registry.HandleSendAction(updatePermissionAction, permissionManager);

			Assert.True(contract.CheckPermission(permissionManager, permission, contractAddress));
			Assert.True(contract.CheckPermission(address, permission, contractAddress));
		}

		[Fact]
		public void AddPermissionReceiver_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			PermittedFavoriteNumberContract contract = new PermittedFavoriteNumberContract(contractAddress, permissionManager);
			this.registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				permission,
				permissionManager);

			this.registry.HandleSendAction(addPermissionAction, permissionManager);
			Address newReceiver = this.addressFactory.Create();

			Assert.True(contract.CheckPermission(permissionManager, permission, contractAddress));

			var updatePermissionAction = new UpdatePermissionAction(
				string.Empty,
				contractAddress,
				new AddressWildCard() { permissionManager },
				new AddressWildCard() { contractAddress },
				permission,
				new AddressWildCard() { permissionManager },
				new AddressWildCard() { contractAddress, newReceiver });

			this.registry.HandleSendAction(updatePermissionAction, permissionManager);

			Assert.True(contract.CheckPermission(permissionManager, permission, newReceiver));
			Assert.True(contract.CheckPermission(permissionManager, permission, contractAddress));
		}

		[Fact]
		public void RemovePermissionSender_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			PermittedFavoriteNumberContract contract = new PermittedFavoriteNumberContract(contractAddress, permissionManager);
			this.registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			Address permitedAddress = this.addressFactory.Create();
			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				permission,
				new AddressWildCard() { permissionManager, permitedAddress });

			this.registry.HandleSendAction(addPermissionAction, permissionManager);

			AnyWildCard anyWildCard = new AnyWildCard();
			var updatePermissionAction = new UpdatePermissionAction(
				string.Empty,
				contractAddress,
				new AddressWildCard() { permissionManager, permitedAddress },
				new AddressWildCard() { contractAddress },
				permission,
				new AddressWildCard() { permissionManager },
				new AddressWildCard() { contractAddress });

			this.registry.HandleSendAction(updatePermissionAction, permissionManager);
			Assert.False(contract.CheckPermission(permitedAddress, permission, contractAddress));
		}

		[Fact]
		public void RemovePermissionReceiver_WhenRemovingSingleAddress_ReturnsFalse()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			PermittedFavoriteNumberContract contract = new PermittedFavoriteNumberContract(contractAddress, permissionManager);
			this.registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			Address nextAddress1 = this.addressFactory.Create();
			Address nextAddress2 = this.addressFactory.Create();
			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				permission,
				new AddressWildCard() { permissionManager },
				new AddressWildCard() { nextAddress1, nextAddress2 });

			this.registry.HandleSendAction(addPermissionAction, permissionManager);

			AnyWildCard anyWildCard = new AnyWildCard();
			var updatePermissionAction = new UpdatePermissionAction(
				string.Empty,
				contractAddress,
				new AddressWildCard() { permissionManager },
				new AddressWildCard() { nextAddress1, nextAddress2 },
				permission,
				new AddressWildCard() { permissionManager },
				new AddressWildCard() { nextAddress2 });

			this.registry.HandleSendAction(updatePermissionAction, permissionManager);
			Assert.False(contract.CheckPermission(permissionManager, permission, nextAddress1));
			Assert.True(contract.CheckPermission(permissionManager, permission, nextAddress2));
		}

		[Fact]
		public void RemovePermissionNextAddress_WhenRemovingWildCard_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			PermittedFavoriteNumberContract contract = new PermittedFavoriteNumberContract(contractAddress, permissionManager);
			this.registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			Address address = this.addressFactory.Create();
			var anyWildCard = new AnyWildCard();

			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				permission,
				new AddressWildCard() { permissionManager });

			this.registry.HandleSendAction(addPermissionAction, permissionManager);
			Assert.True(contract.CheckPermission(permissionManager, permission, contractAddress));

			addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				permission,
				new AddressWildCard() { permissionManager },
				anyWildCard);

			this.registry.HandleSendAction(addPermissionAction, permissionManager);
			Assert.True(contract.CheckPermission(permissionManager, permission, this.addressFactory.Create()));

			var removePermissionAction = new RemovePermissionAction(
				string.Empty,
				contractAddress,
				permission,
				new AddressWildCard() { permissionManager },
				anyWildCard);

			this.registry.HandleSendAction(removePermissionAction, permissionManager);
			Assert.False(contract.CheckPermission(permissionManager, permission, this.addressFactory.Create()));
			Assert.True(contract.CheckPermission(permissionManager, permission, contractAddress));
		}
	}
}