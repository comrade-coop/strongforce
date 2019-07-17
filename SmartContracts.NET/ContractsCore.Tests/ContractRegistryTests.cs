using System;
using System.Collections.Generic;
using System.Data;
using ContractsCore.Contracts;
using ContractsCore.Tests.Mocks;
using Xunit;
using Action = ContractsCore.Actions.Action;

namespace ContractsCore.Tests
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

			registry.RegisterContract(contract);

			Assert.Equal(contract, registry.GetContract(contractAddress));
		}

		[Fact]
		public void RegisterContract_WhenPassedTwoContractsWithSameAddress_ThrowsArgumentException()
		{
			var registry = new ContractRegistry();
			Address contractsAddress = this.addressFactory.Create();
			Contract firstContract = new FavoriteNumberContract();
			Contract secondContract = new FavoriteNumberContract();

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
			var contract = new FavoriteNumberContract();

			registry.RegisterContract(contract);

			var numberAction = new SetFavoriteNumberAction(
				string.Empty,
				contractAddress,
				0);
			contract.SetNumberInvoke(numberAction);
			Assert.Equal(contract.LastOrigin, contractAddress);
			Assert.Equal(contract.LastSender, contractAddress);
		}

		[Fact]
		public void HandleAction_WhenPassedNull_ThrowsArgumentNullException()
		{
			var registry = new ContractRegistry();
			Address senderAddress = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			var contract = new FavoriteNumberContract();

			registry.RegisterContract(contract);

			Assert.Throws<ArgumentNullException>(() => contract.SetNumberInvoke(null));
		}

		[Fact]
		public void HandleAction_WhenPassedActionWithNonExistentAddress_ReturnsFalse()
		{
			var registry = new ContractRegistry();
			Address contractAddress = this.addressFactory.Create();
			var contract = new FavoriteNumberContract();

			registry.RegisterContract(contract);
			Address address = this.addressFactory.Create();
			var action = new SetFavoriteNumberAction(
				string.Empty,
				null, 50);

			Assert.Throws<ArgumentNullException>(() => contract.SetNumberInvoke(action));
		}
	}
}