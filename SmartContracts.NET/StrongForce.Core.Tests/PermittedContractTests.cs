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
		private ContractRegistry registry;

		public PermittedContractTests()
		{
			this.registry = new ContractRegistry();
		}

		[Fact]
		public void Permissions_WhenContractInitialized_ReturnTrue()
		{
			Address permissionManager = this.registry.AddressFactory.Create();
			Address contractAddress = this.registry.CreateContract<FavoriteNumberContract>(permissionManager);

			Contract contract = this.registry.GetContract(contractAddress);

			Assert.True(contract.CheckPermission(permissionManager, typeof(AddPermissionAction), contractAddress));
			Assert.True(contract.CheckPermission(permissionManager, typeof(RemovePermissionAction), contractAddress));
		}

		[Fact]
		public void Receive_WhenPassedActionWithNoPermissions_ThrowsNoPermissionException()
		{
			Address permissionManager = this.registry.AddressFactory.Create();
			Address contractAddress = this.registry.CreateContract<FavoriteNumberContract>(permissionManager);

			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				typeof(AddPermissionAction),
				contractAddress);

			Assert.Throws<NoPermissionException>(() => this.registry.HandleAction(contractAddress, addPermissionAction));
		}

		[Fact]
		public void Receive_WhenPassedSupportedActionWithPermissions_ReturnsTrue()
		{
			Address permissionManager = this.registry.AddressFactory.Create();
			Address contractAddress = this.registry.CreateContract<FavoriteNumberContract>(permissionManager);

			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				typeof(SetFavoriteNumberAction),
				permissionManager);

			Contract contract = this.registry.GetContract(contractAddress);
			Assert.True(this.registry.HandleAction(permissionManager, addPermissionAction));
			Assert.True(contract.CheckPermission(permissionManager, typeof(SetFavoriteNumberAction), contractAddress));
		}

		[Fact]
		public void Receive_WhenRemovePermitedAction_ReturnsTrue()
		{
			Address permissionManager = this.registry.AddressFactory.Create();
			Address contractAddress = this.registry.CreateContract<FavoriteNumberContract>(permissionManager);

			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				typeof(SetFavoriteNumberAction),
				permissionManager);

			var removePermissionAction = new RemovePermissionAction(
				contractAddress,
				typeof(SetFavoriteNumberAction),
				permissionManager);

			Contract contract = this.registry.GetContract(contractAddress);

			Assert.True(this.registry.HandleAction(permissionManager, addPermissionAction));
			Assert.True(contract.CheckPermission(permissionManager, typeof(SetFavoriteNumberAction), contractAddress));
			Assert.True(this.registry.HandleAction(permissionManager, removePermissionAction));
			Assert.False(contract.CheckPermission(permissionManager, typeof(SetFavoriteNumberAction), contractAddress));
		}

		[Fact]
		public void Receive_WhenPassedUnsupportedActionWithPermissions_ReturnsFalse()
		{
			Address permissionManager = this.registry.AddressFactory.Create();
			Address contractAddress = this.registry.CreateContract<FavoriteNumberContract>(permissionManager);

			var addPermissionAction = new Action(contractAddress);

			Assert.Throws<NoPermissionException>(
				() => this.registry.HandleAction(permissionManager, addPermissionAction));
		}

		[Fact]
		public void AddPermission_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.registry.AddressFactory.Create();
			Address contractAddress = this.registry.CreateContract<FavoriteNumberContract>(permissionManager);

			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				typeof(Action),
				permissionManager);

			this.registry.HandleAction(permissionManager, addPermissionAction);

			addPermissionAction = new AddPermissionAction(
				contractAddress,
				typeof(Action),
				null);

			this.registry.HandleAction(permissionManager, addPermissionAction);

			Contract contract = this.registry.GetContract(contractAddress);
			Assert.True(contract.CheckPermission(this.registry.AddressFactory.Create(), typeof(Action), contractAddress));
		}

		[Fact]
		public void UpdatePermission_AddAddressWhenPermissionExists_ReturnsTrue()
		{
			Address address = this.registry.AddressFactory.Create();
			Address permissionManager = this.registry.AddressFactory.Create();
			Address contractAddress = this.registry.CreateContract<FavoriteNumberContract>(permissionManager);

			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				typeof(Action),
				permissionManager);

			this.registry.HandleAction(permissionManager, addPermissionAction);

			var updatePermissionAction = new AddPermissionAction(
				contractAddress,
				typeof(Action),
				address,
				contractAddress);

			this.registry.HandleAction(permissionManager, updatePermissionAction);

			Contract contract = this.registry.GetContract(contractAddress);
			Assert.True(contract.CheckPermission(permissionManager, typeof(Action), contractAddress));
			Assert.True(contract.CheckPermission(address, typeof(Action), contractAddress));
		}

		[Fact]
		public void ForwardAction_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.registry.AddressFactory.Create();
			Address contract1Address = this.registry.CreateContract<FavoriteNumberContract>(permissionManager);
			Address contract2Address = this.registry.CreateContract<FavoriteNumberContract>(contract1Address);
			int testedNumber = 424;

			var intermediaryPermissionAction = new AddPermissionAction(
				contract1Address,
				typeof(SetFavoriteNumberAction),
				permissionManager,
				contract2Address);

			this.registry.HandleAction(permissionManager, intermediaryPermissionAction);

			var forwardAction = new ForwardAction(
				new Address[] { contract1Address },
				new SetFavoriteNumberAction(contract2Address, testedNumber));

			this.registry.HandleAction(permissionManager, forwardAction);

			FavoriteNumberContract contract2 = (FavoriteNumberContract)this.registry.GetContract(contract2Address);
			Assert.Equal(testedNumber, contract2.Number);
		}

		[Fact]
		public void ForwardAction_WithoutIntermediaryPermission_ThrowsNoPermissionException()
		{
			Address permissionManager = this.registry.AddressFactory.Create();
			Address contract1Address = this.registry.CreateContract<FavoriteNumberContract>(permissionManager);
			Address contract2Address = this.registry.CreateContract<FavoriteNumberContract>(contract1Address);
			int testedNumber = 424;

			var forwardAction = new ForwardAction(
				new Address[] { contract1Address },
				new SetFavoriteNumberAction(contract2Address, testedNumber));

			Assert.Throws<NoPermissionException>(() => this.registry.HandleAction(permissionManager, forwardAction));
		}

		[Fact]
		public void ForwardAction_WithoutFinalPermission_ThrowsNoPermissionException()
		{
			Address permissionManager = this.registry.AddressFactory.Create();
			Address contract1Address = this.registry.CreateContract<FavoriteNumberContract>(permissionManager);
			Address contract2Address = this.registry.CreateContract<FavoriteNumberContract>(contract1Address);
			int testedNumber = 424;

			var intermediaryPermissionAction = new AddPermissionAction(
				contract1Address,
				typeof(SetFavoriteNumberAction),
				permissionManager,
				contract2Address);

			this.registry.HandleAction(permissionManager, intermediaryPermissionAction);

			var finalRemovePermissionAction = new RemovePermissionAction(
				contract2Address,
				typeof(SetFavoriteNumberAction),
				contract1Address);

			this.registry.HandleAction(contract1Address, finalRemovePermissionAction);

			var forwardAction = new ForwardAction(
				new Address[] { contract1Address },
				new SetFavoriteNumberAction(contract2Address, testedNumber));

			Assert.Throws<NoPermissionException>(() => this.registry.HandleAction(permissionManager, forwardAction));
		}

		[Fact]
		public void RemovePermissionSender_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.registry.AddressFactory.Create();
			Address contractAddress = this.registry.CreateContract<FavoriteNumberContract>(permissionManager);

			Address permitedAddress = this.registry.AddressFactory.Create();
			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				typeof(Action),
				permissionManager);

			this.registry.HandleAction(permissionManager, addPermissionAction);
			addPermissionAction = new AddPermissionAction(
				contractAddress,
				typeof(Action),
				permitedAddress);

			this.registry.HandleAction(permissionManager, addPermissionAction);
			var updatePermissionAction = new RemovePermissionAction(
				contractAddress,
				typeof(Action),
				permissionManager,
				contractAddress);

			this.registry.HandleAction(permissionManager, updatePermissionAction);

			Contract contract = this.registry.GetContract(contractAddress);
			Assert.True(contract.CheckPermission(permitedAddress, typeof(Action), contractAddress));
			Assert.False(contract.CheckPermission(permissionManager, typeof(Action), contractAddress));
		}
	}
}