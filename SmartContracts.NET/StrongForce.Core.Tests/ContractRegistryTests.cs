using System;
using StrongForce.Core.Tests.Mocks;
using Xunit;

namespace StrongForce.Core.Tests
{
	public class ContractRegistryTests
	{
		private readonly IAddressFactory addressFactory;

		public ContractRegistryTests()
		{
			this.addressFactory = new RandomAddressFactory();
		}

		[Fact]
		public void RegisterContract_WhenPassedNewContract_AddsItCorrectly()
		{
			var registry = new ContractRegistry();
			Address contractAddress = this.addressFactory.Create();
			Contract contract = new FavoriteNumberContract();

			registry.RegisterContract(contractAddress, contract);

			Assert.Equal(contractAddress, contract.Address);
			Assert.Equal(contract, registry.GetContract(contractAddress));
		}

		[Fact]
		public void RegisterContract_WhenPassedTwoContractsWithSameAddress_ThrowsArgumentException()
		{
			var registry = new ContractRegistry();
			Address contractsAddress = this.addressFactory.Create();
			Contract firstContract = new FavoriteNumberContract();
			Contract secondContract = new FavoriteNumberContract();

			registry.RegisterContract(contractsAddress, firstContract);

			Assert.Throws<InvalidOperationException>(() => registry.RegisterContract(contractsAddress, secondContract));
		}

		[Fact]
		public void CreateContract_WhenPassedNewContract_AddsItCorrectly()
		{
			var registry = new ContractRegistry();

			Address contractAddress = registry.CreateContract(typeof(FavoriteNumberContract));

			Assert.NotNull(registry.GetContract(contractAddress));
			Assert.Equal(typeof(FavoriteNumberContract), registry.GetContract(contractAddress).GetType());
		}

		[Fact]
		public void CreateContract_WhenPassedNonContractType_ThrowsArgumentException()
		{
			var registry = new ContractRegistry();

			Assert.Throws<ArgumentOutOfRangeException>(() => registry.CreateContract(typeof(Action)));
		}

		[Fact]
		public void RegisterContract_WhenPassedNull_ThrowsArgumentNullException()
		{
			Address address = this.addressFactory.Create();
			var registry = new ContractRegistry();
			Assert.Throws<ArgumentNullException>(() => registry.RegisterContract(address, null));
		}

		[Fact]
		public void GetContract_WhenPassedNotRegisteredContract_ReturnsNull()
		{
			var registry = new ContractRegistry();
			Address contractAddress = this.addressFactory.Create();

			Assert.Null(registry.GetContract(contractAddress));
		}

		[Fact]
		public void GetContract_WhenPassedNull_ThrowsArgumentNullException()
		{
			var registry = new ContractRegistry();
			Assert.Throws<ArgumentNullException>(() => registry.GetContract(null));
		}

		[Fact]
		public void HandleAction_WhenPassedValidAction_SendsActionToCorrectContract()
		{
			var registry = new ContractRegistry();
			Address senderAddress = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			var contract = new FavoriteNumberContract(senderAddress);

			registry.RegisterContract(contractAddress, contract);

			var numberAction = new SetFavoriteNumberAction(contractAddress, 0);
			registry.HandleAction(senderAddress, numberAction);
			Assert.Equal(contract.LastOrigin, senderAddress);
			Assert.Equal(contract.LastSender, senderAddress);
		}

		[Fact]
		public void HandleAction_WhenPassedNull_ThrowsArgumentNullException()
		{
			var registry = new ContractRegistry();
			Address senderAddress = this.addressFactory.Create();

			Assert.Throws<ArgumentNullException>(() => registry.HandleAction(senderAddress, null));
		}

		[Fact]
		public void HandleAction_WhenPassedActionWithNonExistentAddress_ReturnsFalse()
		{
			var registry = new ContractRegistry();

			Address address = this.addressFactory.Create();
			var action = new SetFavoriteNumberAction(
				null,
				50);

			Assert.Throws<ArgumentNullException>(() => registry.HandleAction(address, action));
		}
	}
}