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
			Contract contract = new FavoriteNumberContract(contractAddress);

			registry.RegisterContract(contract);

			Assert.Equal(contract, registry.GetContract(contractAddress));
		}

		[Fact]
		public void RegisterContract_WhenPassedTwoContractsWithSameAddress_ThrowsArgumentException()
		{
			var registry = new ContractRegistry();
			Address contractsAddress = this.addressFactory.Create();
			Contract firstContract = new FavoriteNumberContract(contractsAddress);
			Contract secondContract = new FavoriteNumberContract(contractsAddress);

			registry.RegisterContract(firstContract);

			Assert.Throws<ArgumentException>(() => registry.RegisterContract(secondContract));
		}

		[Fact]
		public void RegisterContract_WhenPassedNull_ThrowsArgumentNullException()
		{
			var registry = new ContractRegistry();
			Assert.Throws<ArgumentNullException>(() => registry.RegisterContract(null));
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
			var contract = new FavoriteNumberContract(contractAddress, senderAddress);

			registry.RegisterContract(contract);

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
			Address contractAddress = this.addressFactory.Create();
			var contract = new FavoriteNumberContract(contractAddress);

			registry.RegisterContract(contract);
			Address address = this.addressFactory.Create();
			var action = new SetFavoriteNumberAction(
				null,
				50);

			Assert.Throws<ArgumentNullException>(() => registry.HandleAction(address, action));
		}
	}
}