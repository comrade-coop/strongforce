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
		private readonly IAddressFactory AddressFactory;
		private ContractRegistryMock Registry;

		public PermittedFavoriteNumberContractTests()
		{
			this.AddressFactory = new RandomAddressFactory();
			this.Registry = new ContractRegistryMock();
		}

		[Fact]
		public void Receive_WhenPassedSetFavoriteNumberActionWithGrantedPermissions_SetsNumberCorrectly()
		{
			Address contractAddress = this.AddressFactory.Create();
			Address permissionManager = this.AddressFactory.Create();
			var contract = new PermittedFavoriteNumberContract(contractAddress, Registry, permissionManager);
			this.Registry.RegisterContract(contract);

			var addPermissionAction = new AddPermissionAction(
				string.Empty,
				contractAddress,
				new Permission(typeof(SetFavoriteNumberAction)),
				permissionManager);

			const int expectedNumber = 32;
			var setNumberAction = new SetFavoriteNumberAction(
				string.Empty,
				contractAddress,
				expectedNumber);

			Assert.True(this.Registry.HandleSendAction(addPermissionAction, permissionManager));
			Assert.True(this.Registry.HandleSendAction(setNumberAction, permissionManager));
			Assert.Equal(expectedNumber, contract.Number);
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