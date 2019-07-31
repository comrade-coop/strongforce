using System.Collections.Generic;
using System.Reflection;
using StrongForce.Core.Exceptions;
using StrongForce.Core.Permissions;
using StrongForce.Core.Tests.Mocks;
using Xunit;

namespace StrongForce.Core.Tests
{
	public class PermittedContractTests
	{
		private readonly IAddressFactory addressFactory;
		private ContractRegistry registry;

		public PermittedContractTests()
		{
			this.addressFactory = new RandomAddressFactory();
			this.registry = new ContractRegistry();
		}

		[Fact]
		public void Permissions_WhenContractInitialized_ReturnTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			FavoriteNumberContract contract = new FavoriteNumberContract(contractAddress, permissionManager);

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
			Contract contract = new FavoriteNumberContract(contractAddress, permissionManager);

			this.registry.RegisterContract(contract);

			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				new Permission(typeof(AddPermissionAction)),
				contractAddress).ConfigureSenderAndOrigin(contractAddress);

			Assert.Throws<NoPermissionException>(() => this.registry.HandleAction(addPermissionAction));
		}

		[Fact]
		public void Receive_WhenPassedSupportedActionWithPermissions_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			FavoriteNumberContract contract = new FavoriteNumberContract(contractAddress, permissionManager);
			this.registry.RegisterContract(contract);
			var numberPermission = new Permission(typeof(SetFavoriteNumberAction));
			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				numberPermission,
				permissionManager).ConfigureSenderAndOrigin(permissionManager);

			Assert.True(this.registry.HandleAction(addPermissionAction));
			Assert.True(contract.CheckPermission(permissionManager, numberPermission, contractAddress));
		}

		[Fact]
		public void Receive_WhenRemovePermitedAction_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			FavoriteNumberContract contract = new FavoriteNumberContract(contractAddress, permissionManager);
			this.registry.RegisterContract(contract);
			var numberPermission = new Permission(typeof(SetFavoriteNumberAction));
			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				numberPermission,
				permissionManager).ConfigureSenderAndOrigin(permissionManager);

			var removeAddPermissionAction = new RemovePermissionAction(
				contractAddress,
				numberPermission,
				contractAddress).ConfigureSenderAndOrigin(permissionManager);

			Assert.True(this.registry.HandleAction(addPermissionAction));
			Assert.True(contract.CheckPermission(permissionManager, numberPermission, contractAddress));
			Assert.True(this.registry.HandleAction(removeAddPermissionAction));
			Assert.False(contract.CheckPermission(permissionManager, numberPermission, contractAddress));
		}

		[Fact]
		public void Receive_WhenPassedUnsupportedActionWithPermissions_ReturnsFalse()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			Contract contract = new FavoriteNumberContract(contractAddress, permissionManager);
			this.registry.RegisterContract(contract);

			var addPermissionAction = new Action(contractAddress).ConfigureSenderAndOrigin(permissionManager);

			Assert.Throws<NoPermissionException>(
				() => this.registry.HandleAction(addPermissionAction));
		}

		[Fact]
		public void AddPermission_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			FavoriteNumberContract contract = new FavoriteNumberContract(contractAddress, permissionManager);
			this.registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				permissionManager).ConfigureSenderAndOrigin(permissionManager);

			this.registry.HandleAction(addPermissionAction);

			addPermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				Address.Null).ConfigureSenderAndOrigin(permissionManager);
			this.registry.HandleAction(addPermissionAction);
			Assert.True(contract.CheckPermission(this.addressFactory.Create(), permission, contractAddress));
		}

		[Fact]
		public void UpdatePermission_AddAddressWhenPermissionExists_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			FavoriteNumberContract contract = new FavoriteNumberContract(contractAddress, permissionManager);
			this.registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				permissionManager).ConfigureSenderAndOrigin(permissionManager);

			this.registry.HandleAction(addPermissionAction);

			var updatePermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				address,
				contractAddress).ConfigureSenderAndOrigin(permissionManager);

			this.registry.HandleAction(updatePermissionAction);

			Assert.True(contract.CheckPermission(permissionManager, permission, contractAddress));
			Assert.True(contract.CheckPermission(address, permission, contractAddress));
		}

		[Fact]
		public void ForwardAction_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contract1Address = this.addressFactory.Create();
			Address contract2Address = this.addressFactory.Create();
			int testedNumber = 424;

			FavoriteNumberContract contract1 = new FavoriteNumberContract(contract1Address, permissionManager);
			this.registry.RegisterContract(contract1);

			FavoriteNumberContract contract2 = new FavoriteNumberContract(contract2Address, permissionManager);
			this.registry.RegisterContract(contract2);

			var intermediaryPermissionAction = new AddPermissionAction(
				contract1Address,
				new Permission(typeof(SetFavoriteNumberAction)),
				permissionManager,
				contract2Address)
				.ConfigureSenderAndOrigin(permissionManager);

			this.registry.HandleAction(intermediaryPermissionAction);

			var finalPermissionAction = new AddPermissionAction(
				contract2Address,
				new Permission(typeof(SetFavoriteNumberAction)),
				contract1Address)
				.ConfigureSenderAndOrigin(permissionManager);

			this.registry.HandleAction(finalPermissionAction);

			var forwardAction = new ForwardAction(
				new Address[] { contract1Address },
				new SetFavoriteNumberAction(contract2Address, testedNumber))
				.ConfigureSenderAndOrigin(permissionManager);

			this.registry.HandleAction(forwardAction);

			Assert.Equal(testedNumber, contract2.Number);
		}

		[Fact]
		public void ForwardAction_WithoutIntermediaryPermission_ThrowsNoPermissionException()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contract1Address = this.addressFactory.Create();
			Address contract2Address = this.addressFactory.Create();
			int testedNumber = 424;

			FavoriteNumberContract contract1 = new FavoriteNumberContract(contract1Address, permissionManager);
			this.registry.RegisterContract(contract1);

			FavoriteNumberContract contract2 = new FavoriteNumberContract(contract2Address, permissionManager);
			this.registry.RegisterContract(contract2);

			var finalPermissionAction = new AddPermissionAction(
				contract2Address,
				new Permission(typeof(SetFavoriteNumberAction)),
				contract1Address)
				.ConfigureSenderAndOrigin(permissionManager);

			this.registry.HandleAction(finalPermissionAction);

			var forwardAction = new ForwardAction(
				new Address[] { contract1Address },
				new SetFavoriteNumberAction(contract2Address, testedNumber))
				.ConfigureSenderAndOrigin(permissionManager);

			Assert.Throws<NoPermissionException>(() => this.registry.HandleAction(forwardAction));
		}

		[Fact]
		public void ForwardAction_WithoutFinalPermission_ThrowsNoPermissionException()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contract1Address = this.addressFactory.Create();
			Address contract2Address = this.addressFactory.Create();
			int testedNumber = 424;

			FavoriteNumberContract contract1 = new FavoriteNumberContract(contract1Address, permissionManager);
			this.registry.RegisterContract(contract1);

			FavoriteNumberContract contract2 = new FavoriteNumberContract(contract2Address, permissionManager);
			this.registry.RegisterContract(contract2);

			var intermediaryPermissionAction = new AddPermissionAction(
				contract1Address,
				new Permission(typeof(SetFavoriteNumberAction)),
				permissionManager,
				contract2Address)
				.ConfigureSenderAndOrigin(permissionManager);

			this.registry.HandleAction(intermediaryPermissionAction);

			var forwardAction = new ForwardAction(
				new Address[] { contract1Address },
				new SetFavoriteNumberAction(contract2Address, testedNumber))
				.ConfigureSenderAndOrigin(permissionManager);

			Assert.Throws<NoPermissionException>(() => this.registry.HandleAction(forwardAction));
		}

		[Fact]
		public void ForwardAction_WithoutForwarding_ThrowsUnknownOriginException()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contract1Address = this.addressFactory.Create();
			Address contract2Address = this.addressFactory.Create();
			int testedNumber = 424;

			FavoriteNumberContract contract1 = new FavoriteNumberContract(contract1Address, permissionManager);
			this.registry.RegisterContract(contract1);

			FavoriteNumberContract contract2 = new FavoriteNumberContract(contract2Address, permissionManager);
			this.registry.RegisterContract(contract2);

			var finalPermissionAction = new AddPermissionAction(
				contract2Address,
				new Permission(typeof(SetFavoriteNumberAction)),
				contract1Address)
				.ConfigureSenderAndOrigin(permissionManager);

			this.registry.HandleAction(finalPermissionAction);

			var forwardAction = new ForwardAction(
				new Address[] { contract1Address },
				new SetFavoriteNumberAction(contract2Address, testedNumber))
				.ConfigureSenderAndOrigin(permissionManager) as ForwardAction;

			Assert.Throws<UnknownActionOriginException>(() => this.registry.HandleAction(forwardAction.NextAction));
		}

		[Fact]
		public void ForwardAction_WithFakeOrigin_ThrowsUnknownOriginException()
		{
			Address aliceAddress = this.addressFactory.Create();
			Address bobAddress = this.addressFactory.Create();
			Address eveAddress = this.addressFactory.Create();

			// Eve should be unable to trick Alice from thinking the message originates from Bob
			var forwardAction = new ForwardAction(
				bobAddress,
				new Action(aliceAddress))
				.ConfigureSenderAndOrigin(eveAddress);

			// Some reflection magic used in order to circumvent .ConfigureOrigin recursively calling .ConfigureOrigin on .NextAction in the current implementation.
			// Note that the the same might be (unlikely) done by a deserialization library
			var type = typeof(Action);
			foreach (var field in type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
			{
				if (field.Name.ToLower().Contains("origin"))
				{
					field.SetValue(forwardAction, bobAddress);
					break;
				}
			}

			Assert.Throws<UnknownActionOriginException>(() => this.registry.HandleAction(forwardAction));
		}

		/*[Fact]
		public void AddPermissionReceiver_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			FavoriteNumberContract contract = new FavoriteNumberContract(contractAddress, permissionManager);
			this.registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				permissionManager).ConfigureSenderAndOrigin(permissionManager);

			this.registry.HandleAction(addPermissionAction);
			Address newReceiver = this.addressFactory.Create();

			Assert.True(contract.CheckPermission(permissionManager, permission, contractAddress));

			var updatePermissionAction = new RemovePermittedAddressAction(
				contractAddress,
				permission,
				new HashSet<Address> { permissionManager },
				new HashSet<Address> { contractAddress, newReceiver }
				contractAddress).ConfigureSenderAndOrigin(permissionManager);

			this.registry.HandleAction(updatePermissionAction);

			Assert.True(contract.CheckPermission(permissionManager, permission, newReceiver));
			Assert.True(contract.CheckPermission(permissionManager, permission, contractAddress));
		}*/

		[Fact]
		public void RemovePermissionSender_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			FavoriteNumberContract contract = new FavoriteNumberContract(contractAddress, permissionManager);
			this.registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			Address permitedAddress = this.addressFactory.Create();
			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				permissionManager).ConfigureSenderAndOrigin(permissionManager);

			this.registry.HandleAction(addPermissionAction);
			addPermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				permitedAddress).ConfigureSenderAndOrigin(permissionManager);

			this.registry.HandleAction(addPermissionAction);
			var updatePermissionAction = new RemovePermittedAddressAction(
				contractAddress,
				permission,
				permissionManager,
				contractAddress).ConfigureSenderAndOrigin(permissionManager);

			this.registry.HandleAction(updatePermissionAction);
			Assert.True(contract.CheckPermission(permitedAddress, permission, contractAddress));
			Assert.False(contract.CheckPermission(permissionManager, permission, contractAddress));
		}

		/*[Fact]
		public void RemovePermissionReceiver_WhenRemovingSingleAddress_ReturnsFalse()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			FavoriteNumberContract contract = new FavoriteNumberContract(contractAddress, permissionManager);
			this.registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			Address nextAddress1 = this.addressFactory.Create();
			Address nextAddress2 = this.addressFactory.Create();
			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				new HashSet<Address> { permissionManager },
				new HashSet<Address> { nextAddress1, nextAddress2 }).ConfigureSenderAndOrigin(permissionManager);

			this.registry.HandleAction(addPermissionAction);

			var updatePermissionAction = new RemovePermittedAddressAction(
				contractAddress,
				new HashSet<Address> { permissionManager },
				permission,
				new HashSet<Address> { permissionManager },
				new HashSet<Address> { nextAddress2 }).ConfigureSenderAndOrigin(permissionManager);

			this.registry.HandleAction(updatePermissionAction);
			Assert.False(contract.CheckPermission(permissionManager, permission, nextAddress1));
			Assert.True(contract.CheckPermission(permissionManager, permission, nextAddress2));
		}*/

		/*[Fact]
		public void RemovePermissionPermitedAddress_WhenRemovingWildCard_ReturnsTrue()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			FavoriteNumberContract contract = new FavoriteNumberContract(contractAddress, permissionManager);
			this.registry.RegisterContract(contract);
			var permission = new Permission(typeof(Action));
			Address address = this.addressFactory.Create();

			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				new HashSet<Address> { permissionManager }).ConfigureSenderAndOrigin(permissionManager);

			this.registry.HandleAction(addPermissionAction);
			Assert.True(contract.CheckPermission(permissionManager, permission, contractAddress));

			addPermissionAction = new AddPermissionAction(
				contractAddress,
				permission,
				new HashSet<Address> { permissionManager },
				this.anyWildCard).ConfigureSenderAndOrigin(permissionManager);

			this.registry.HandleAction(addPermissionAction);
			Assert.True(contract.CheckPermission(permissionManager, permission, this.addressFactory.Create()));

			var removePermissionAction = new RemovePermittedAddressAction(
				contractAddress,
				new HashSet<Address> { permissionManager },
				permission,
				new HashSet<Address> { permissionManager },
				new HashSet<Address> { contractAddress }).ConfigureSenderAndOrigin(permissionManager);

			this.registry.HandleAction(removePermissionAction);
			Assert.False(contract.CheckPermission(permissionManager, permission, this.addressFactory.Create()));
			Assert.True(contract.CheckPermission(permissionManager, permission, contractAddress));
		}*/
	}
}