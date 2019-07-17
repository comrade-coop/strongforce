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
		private readonly ContractRegistry registry;

		public ContractRegistryTests()
		{
			this.addressFactory = new RandomAddressFactory();
			ContractRegistry.Initialise(this.addressFactory);
			this.registry = ContractRegistry.Instance;
		}

		[Fact]
		public void RegisterContract_WhenPassedNewContract_AddsItCorrectly()
		{
			Address contractAddress = this.addressFactory.Create();
			Contract contract = new FavoriteNumberContract(contractAddress);

			this.registry.RegisterContract(contract);

			Assert.Equal(contract, this.registry.GetContract(contractAddress));
		}

		[Fact]
		public void RegisterContract_WhenPassedTwoContractsWithSameAddress_ThrowsArgumentException()
		{
			Address contractsAddress = this.addressFactory.Create();
			Contract firstContract = new FavoriteNumberContract(contractsAddress);
			Contract secondContract = new FavoriteNumberContract(contractsAddress);

			this.registry.RegisterContract(firstContract);

			Assert.Throws<ArgumentException>(() => this.registry.RegisterContract(secondContract));
		}

		[Fact]
		public void RegisterContract_WhenPassedNull_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => this.registry.RegisterContract(null));
		}

		[Fact]
		public void GetContract_WhenPassedNotRegisteredContract_ReturnsNull()
		{
			Address contractAddress = this.addressFactory.Create();

			Assert.Null(this.registry.GetContract(contractAddress));
		}

		[Fact]
		public void GetContract_WhenPassedNull_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => this.registry.GetContract(null));
		}

		[Fact]
		public void HandleAction_WhenPassedValidAction_SendsActionToCorrectContract()
		{
			Address senderAddress = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			var contract = new FavoriteNumberContract(contractAddress);

			this.registry.RegisterContract(contract);

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
			Address senderAddress = this.addressFactory.Create();
			Address contractAddress = this.addressFactory.Create();
			var contract = new FavoriteNumberContract(contractAddress);

			this.registry.RegisterContract(contract);

			Assert.Throws<ArgumentNullException>(() => contract.SetNumberInvoke(null));
		}

		[Fact]
		public void HandleAction_WhenPassedActionWithNonExistentAddress_ReturnsFalse()
		{
			Address contractAddress = this.addressFactory.Create();
			var contract = new FavoriteNumberContract(contractAddress);

			this.registry.RegisterContract(contract);
			Address address = this.addressFactory.Create();
			var action = new SetFavoriteNumberAction(
				string.Empty,
				null, 50);

			Assert.Throws<ArgumentNullException>(() => contract.SetNumberInvoke(action));
		}
	}
}