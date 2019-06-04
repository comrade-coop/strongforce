using StrongForce.Core.Exceptions;
using StrongForce.Core.Permissions;
using StrongForce.Core.Tests.Mocks;
using Xunit;

namespace StrongForce.Core.Tests
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
			PermittedFavoriteNumberContract contract = new PermittedFavoriteNumberContract(contractAddress, this.Registry, permissionManager);
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
				new AddressWildCard() { permissionManager },
				new AddressWildCard() { contractAddress });

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
			Contract contract = new PermittedFavoriteNumberContract(contractAddress, this.Registry, permissionManager);
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
			PermittedFavoriteNumberContract contract = new PermittedFavoriteNumberContract(contractAddress, Registry, permissionManager);
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
		public void UpdatePermission_WhenPermissionExists_ReturnsTrue()
		{
			Address address = this.AddressFactory.Create();
			Address permissionManager = this.AddressFactory.Create();
			Address contractAddress = this.AddressFactory.Create();
			PermittedFavoriteNumberContract contract = new PermittedFavoriteNumberContract(contractAddress, Registry, permissionManager);
			this.Registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				permission,
				permissionManager);

			this.Registry.HandleSendAction(addPermissionAction, permissionManager);

			var updatePermissionAction = new UpdatePermissionAction(
				string.Empty,
				contractAddress,
				new AddressWildCard() { permissionManager },
				new AddressWildCard() { contractAddress },
				permission,
				new AddressWildCard() { permissionManager, address },
				new AddressWildCard() { contractAddress });

			this.Registry.HandleSendAction(updatePermissionAction, permissionManager);

			Assert.True(contract.CheckPermission(permissionManager, permission, contractAddress));
			Assert.True(contract.CheckPermission(address, permission, contractAddress));
		}

		[Fact]
		public void AddPermissionReceiver_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.AddressFactory.Create();
			Address contractAddress = this.AddressFactory.Create();
			PermittedFavoriteNumberContract contract = new PermittedFavoriteNumberContract(contractAddress, Registry, permissionManager);
			this.Registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				permission,
				permissionManager);

			this.Registry.HandleSendAction(addPermissionAction, permissionManager);
			Address newReceiver = this.AddressFactory.Create();

			Assert.True(contract.CheckPermission(permissionManager, permission, contractAddress));

			var updatePermissionAction = new UpdatePermissionAction(
				string.Empty,
				contractAddress,
				new AddressWildCard() { permissionManager },
				new AddressWildCard() { contractAddress },
				permission,
				new AddressWildCard() { permissionManager },
				new AddressWildCard() { contractAddress, newReceiver });

			this.Registry.HandleSendAction(updatePermissionAction, permissionManager);

			Assert.True(contract.CheckPermission(permissionManager, permission, newReceiver));
			Assert.True(contract.CheckPermission(permissionManager, permission, contractAddress));
		}

		[Fact]
		public void RemovePermissionSender_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.AddressFactory.Create();
			Address contractAddress = this.AddressFactory.Create();
			PermittedFavoriteNumberContract contract = new PermittedFavoriteNumberContract(contractAddress, Registry, permissionManager);
			this.Registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			Address permitedAddress = this.AddressFactory.Create();
			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				permission,
				new AddressWildCard() { permissionManager, permitedAddress });

			this.Registry.HandleSendAction(addPermissionAction, permissionManager);

			AnyWildCard anyWildCard = new AnyWildCard();
			var updatePermissionAction = new UpdatePermissionAction(
				string.Empty,
				contractAddress,
				new AddressWildCard() { permissionManager, permitedAddress },
				new AddressWildCard() { contractAddress },
				permission,
				new AddressWildCard() { permissionManager },
				new AddressWildCard() { contractAddress });

			this.Registry.HandleSendAction(updatePermissionAction, permissionManager);
			Assert.False(contract.CheckPermission(permitedAddress, permission, contractAddress));
		}

		[Fact]
		public void RemovePermissionReceiver_WhenRemovingSingleAddress_ReturnsFalse()
		{
			Address permissionManager = this.AddressFactory.Create();
			Address contractAddress = this.AddressFactory.Create();
			PermittedFavoriteNumberContract contract = new PermittedFavoriteNumberContract(contractAddress, Registry, permissionManager);
			this.Registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			Address nextAddress1 = this.AddressFactory.Create();
			Address nextAddress2 = this.AddressFactory.Create();
			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				permission,
				new AddressWildCard() { permissionManager },
				new AddressWildCard() { nextAddress1, nextAddress2 });

			this.Registry.HandleSendAction(addPermissionAction, permissionManager);

			AnyWildCard anyWildCard = new AnyWildCard();
			var updatePermissionAction = new UpdatePermissionAction(
				string.Empty,
				contractAddress,
				new AddressWildCard() { permissionManager },
				new AddressWildCard() { nextAddress1, nextAddress2 },
				permission,
				new AddressWildCard() { permissionManager },
				new AddressWildCard() { nextAddress2 });

			this.Registry.HandleSendAction(updatePermissionAction, permissionManager);
			Assert.False(contract.CheckPermission(permissionManager, permission, nextAddress1));
			Assert.True(contract.CheckPermission(permissionManager, permission, nextAddress2));
		}

		[Fact]
		public void RemovePermissionNextAddress_WhenRemovingWildCard_ReturnsTrue()
		{
			Address permissionManager = this.AddressFactory.Create();
			Address contractAddress = this.AddressFactory.Create();
			PermittedFavoriteNumberContract contract = new PermittedFavoriteNumberContract(contractAddress, Registry, permissionManager);
			this.Registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			Address address = this.AddressFactory.Create();
			var anyWildCard = new AnyWildCard();

			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				permission,
				new AddressWildCard() { permissionManager });

			this.Registry.HandleSendAction(addPermissionAction, permissionManager);
			Assert.True(contract.CheckPermission(permissionManager, permission, contractAddress));

			addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				permission,
				new AddressWildCard() { permissionManager },
				anyWildCard);

			this.Registry.HandleSendAction(addPermissionAction, permissionManager);
			Assert.True(contract.CheckPermission(permissionManager, permission, this.AddressFactory.Create()));

			var removePermissionAction = new RemovePermissionAction(
				string.Empty,
				contractAddress,
				permission,
				new AddressWildCard() { permissionManager },
				anyWildCard);

			this.Registry.HandleSendAction(removePermissionAction, permissionManager);
			Assert.False(contract.CheckPermission(permissionManager, permission, this.AddressFactory.Create()));
			Assert.True(contract.CheckPermission(permissionManager, permission, contractAddress));
		}
	}
}