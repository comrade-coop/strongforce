using ContractsCore.Actions;
using ContractsCore.Exceptions;
using ContractsCore.Permissions;
using ContractsCore.Tests.Mocks;
using ContractsCore.Contracts;
using Xunit;

namespace ContractsCore.Tests
{
	public class PermittedFavoriteNumberContractTests
	{
		private readonly IAddressFactory addressFactory;
		private readonly ContractRegistryMock registry;

		public PermittedFavoriteNumberContractTests()
		{
			this.addressFactory = new RandomAddressFactory();
			this.registry = new ContractRegistryMock();
		}

		[Fact]
		public void Receive_WhenPassedSetFavoriteNumberActionWithGrantedPermissions_SetsNumberCorrectly()
		{
			Address contractAddress = this.addressFactory.Create();
			Address permissionManager = this.addressFactory.Create();
			var contract = new PermittedFavoriteNumberContract(permissionManager);
			this.registry.RegisterContract(contract);

			var addPermissionAction = new AddPermissionAction(
				contractAddress,
				new Permission(typeof(SetFavoriteNumberAction)),
				permissionManager);

			const int expectedNumber = 32;
			var setNumberAction = new SetFavoriteNumberAction(
				contractAddress,
				expectedNumber);

			Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));
			Assert.True(this.registry.HandleSendAction(setNumberAction, permissionManager));
			Assert.Equal(expectedNumber, contract.Number);
		}

		[Fact]
		public void Receive_WhenPassedUnsupportedActionWithPermissions_ReturnsFalse()
		{
			Address permissionManager = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			Contract contract = new PermittedFavoriteNumberContract(permissionManager);
			this.registry.RegisterContract(contract);

			var addPermissionAction = new Action(
				contractAddress);

			Assert.Throws<NoPermissionException>(
				() => this.registry.HandleSendAction(addPermissionAction, permissionManager));
		}

		// TODO test acl interaction
	}
}