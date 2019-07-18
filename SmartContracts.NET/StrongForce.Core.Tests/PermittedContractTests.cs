using StrongForce.Core.Exceptions;
using StrongForce.Core.Permissions;
using StrongForce.Core.Tests.Mocks;
using System.Collections.Generic;
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
				contractAddress,
				numberPermission,
				permissionManager);

			var removeAddPermissionAction = new RemovePermissionAction(
				contractAddress,
				numberPermission,
				contractAddress);

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

			var addPermissionAction = new Action(contractAddress);

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
				contractAddress,
				permission,
				permissionManager);

			this.registry.HandleSendAction(addPermissionAction, permissionManager);

			addPermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				Address.Null());
			this.registry.HandleSendAction(addPermissionAction, permissionManager);
			Assert.True(contract.CheckPermission(this.addressFactory.Create(), permission, contractAddress));
		}

		[Fact]
		public void UpdatePermission_AddAddressWhenPermissionExists_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			PermittedFavoriteNumberContract contract = new PermittedFavoriteNumberContract(contractAddress, permissionManager);
			this.registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				permissionManager);

			this.registry.HandleSendAction(addPermissionAction, permissionManager);

			var updatePermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				address,
				contractAddress);

			this.registry.HandleSendAction(updatePermissionAction, permissionManager);

			Assert.True(contract.CheckPermission(permissionManager, permission, contractAddress));
			Assert.True(contract.CheckPermission(address, permission, contractAddress));
		}

		/*[Fact]
		public void AddPermissionReceiver_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			PermittedFavoriteNumberContract contract = new PermittedFavoriteNumberContract(contractAddress, permissionManager);
			this.registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				permissionManager);

			this.registry.HandleSendAction(addPermissionAction, permissionManager);
			Address newReceiver = this.addressFactory.Create();

			Assert.True(contract.CheckPermission(permissionManager, permission, contractAddress));

			var updatePermissionAction = new RemovePermittedAddressAction(
				contractAddress,
				permission,
				new HashSet<Address> { permissionManager },
				new HashSet<Address> { contractAddress, newReceiver }
				contractAddress);

			this.registry.HandleSendAction(updatePermissionAction, permissionManager);

			Assert.True(contract.CheckPermission(permissionManager, permission, newReceiver));
			Assert.True(contract.CheckPermission(permissionManager, permission, contractAddress));
		}*/

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
				contractAddress,
				permission,
				permissionManager);

			this.registry.HandleSendAction(addPermissionAction, permissionManager);
			addPermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				permitedAddress);

			this.registry.HandleSendAction(addPermissionAction, permissionManager);
			var updatePermissionAction = new RemovePermittedAddressAction(
				contractAddress,
				permission,
				permissionManager,
				contractAddress);

			this.registry.HandleSendAction(updatePermissionAction, permissionManager);
			Assert.True(contract.CheckPermission(permitedAddress, permission, contractAddress));
			Assert.False(contract.CheckPermission(permissionManager, permission, contractAddress));
		}

		/*[Fact]
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
				contractAddress,
				permission,
				new HashSet<Address> { permissionManager },
				new HashSet<Address> { nextAddress1, nextAddress2 });

			this.registry.HandleSendAction(addPermissionAction, permissionManager);

			var updatePermissionAction = new RemovePermittedAddressAction(
				contractAddress,
				new HashSet<Address> { permissionManager },
				permission,
				new HashSet<Address> { permissionManager },
				new HashSet<Address> { nextAddress2 });

			this.registry.HandleSendAction(updatePermissionAction, permissionManager);
			Assert.False(contract.CheckPermission(permissionManager, permission, nextAddress1));
			Assert.True(contract.CheckPermission(permissionManager, permission, nextAddress2));
		}*/

		/*[Fact]
		public void RemovePermissionPermitedAddress_WhenRemovingWildCard_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			PermittedFavoriteNumberContract contract = new PermittedFavoriteNumberContract(contractAddress, permissionManager);
			this.registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			Address address = this.addressFactory.Create();

			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				new HashSet<Address> { permissionManager });

			this.registry.HandleSendAction(addPermissionAction, permissionManager);
			Assert.True(contract.CheckPermission(permissionManager, permission, contractAddress));

			addPermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				new HashSet<Address> { permissionManager },
				this.anyWildCard);

			this.registry.HandleSendAction(addPermissionAction, permissionManager);
			Assert.True(contract.CheckPermission(permissionManager, permission, this.addressFactory.Create()));

			var removePermissionAction = new RemovePermittedAddressAction(
				contractAddress,
				new HashSet<Address> { permissionManager },
				permission,
				new HashSet<Address> { permissionManager },
				new HashSet<Address> { contractAddress });

			this.registry.HandleSendAction(removePermissionAction, permissionManager);
			Assert.False(contract.CheckPermission(permissionManager, permission, this.addressFactory.Create()));
			Assert.True(contract.CheckPermission(permissionManager, permission, contractAddress));
		}*/
	}
}