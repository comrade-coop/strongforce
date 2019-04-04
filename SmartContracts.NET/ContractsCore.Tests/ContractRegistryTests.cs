using System;
using System.Collections.Generic;
using System.Data;
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
		public void GetContract_WhenPassedNotRegisteredContract_ThrowsKeyNotFoundException()
		{
			var registry = new ContractRegistry();
			Address contractAddress = this.addressFactory.Create();

			Assert.Throws<KeyNotFoundException>(() => registry.GetContract(contractAddress));
		}

		[Fact]
		public void GetContract_WhenPassedNull_ThrowsArgumentNullException()
		{
			var registry = new ContractRegistry();
			Assert.Throws<ArgumentNullException>(() => registry.GetContract(null));
		}

		[Fact]
		public void HandleAction_WhenPassedValidAction_ReturnsTrue()
		{
			var registry = new ContractRegistry();
			Address senderAddress = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			var contract = new FavoriteNumberContract(contractAddress);

			registry.RegisterContract(contract);

			var numberAction = new SetFavoriteNumberAction(
				string.Empty,
				senderAddress,
				senderAddress,
				contractAddress,
				0);

			Assert.True(registry.HandleAction(numberAction));
		}

		[Fact]
		public void HandleAction_WhenPassedValidAction_SendsActionToCorrectContract()
		{
			const int expectedNumber = 342;
			var registry = new ContractRegistry();
			Address senderAddress = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			var contract = new FavoriteNumberContract(contractAddress);

			registry.RegisterContract(contract);

			var numberAction = new SetFavoriteNumberAction(
				string.Empty,
				senderAddress,
				senderAddress,
				contractAddress,
				expectedNumber);

			registry.HandleAction(numberAction);

			Assert.Equal(expectedNumber, contract.Number);
		}

		[Fact]
		public void HandleAction_WhenPassedNull_ThrowsArgumentNullException()
		{
			var registry = new ContractRegistry();
			Assert.Throws<ArgumentNullException>(() => registry.HandleAction(null));
		}

		[Fact]
		public void HandleAction_WhenPassedActionWithNonExistentAddress_ReturnsFalse()
		{
			var registry = new ContractRegistry();
			Address address = this.addressFactory.Create();
			var action = new Action(
				string.Empty,
				address,
				address,
				address);

			Assert.False(registry.HandleAction(action));
		}
	}
}