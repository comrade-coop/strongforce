using System;
using System.Collections.Generic;
using System.Data;
using ContractsCore.Contracts;
using ContractsCore.Exceptions;
using ContractsCore.Tests.Mocks;
using Xunit;
using Action = ContractsCore.Actions.Action;

namespace ContractsCore.Tests
{
	public class ContractRegistryTests
	{
		private readonly IAddressFactory addressFactory;
		private readonly IContractRegistry registry;

		public ContractRegistryTests()
		{
			this.addressFactory = new RandomAddressFactory();
			ContractRegistry.InitialiseInstance(this.addressFactory);
			this.registry = ContractRegistry.GetInstance();
		}

		[Fact]
		public void RegisterContract_WhenPassedNewContract_AddsItCorrectly()
		{
			Contract contract = new FavoriteNumberContract();
			Assert.Equal(contract, this.registry.GetContract<Contract>(contract.Address));
		}

		[Fact]
		public void RegisterContract_WhenPassedSameContractTwice_ThrowsArgumentException()
		{
			Contract firstContract = new FavoriteNumberContract();
			Assert.Throws<ArgumentException>(() => this.registry.RegisterContract(firstContract));
		}

		[Fact]
		public void RegisterContract_WhenPassedNull_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => registry.RegisterContract(null));
		}

		[Fact]
		public void GetContract_WhenPassedNotRegisteredContract_ThrowsKeyNotFoundException()
		{
			Address contractAddress = this.addressFactory.Create();

			Assert.Throws<UnknownContractException>(() => this.registry.GetContract<Contract>(contractAddress));
		}

		[Fact]
		public void GetContract_WhenPassedNull_ThrowsArgumentNullException()
		{
			Assert.Throws<ArgumentNullException>(() => this.registry.GetContract<Contract>(null));
		}

		[Fact]
		public void HandleAction_WhenPassedValidAction_SendsActionToCorrectContract()
		{
			var contract = new FavoriteNumberContract();

			var numberAction = new SetFavoriteNumberAction(contract.Address, 0);
			contract.SetNumberInvoke(numberAction);

			Assert.Equal(contract.LastOrigin, contract.Address);
			Assert.Equal(contract.LastSender, contract.Address);
		}

		[Fact]
		public void HandleAction_WhenPassedNull_ThrowsArgumentNullException()
		{
			var contract = new FavoriteNumberContract();

			Assert.Throws<ArgumentNullException>(() => contract.SetNumberInvoke(null));
		}

		[Fact]
		public void HandleAction_WhenPassedActionWithNonExistentAddress_ReturnsFalse()
		{
			var contract = new FavoriteNumberContract();
			var action = new SetFavoriteNumberAction(
				null,
				50);

			Assert.Throws<ArgumentNullException>(() => contract.SetNumberInvoke(action));
		}
	}
}