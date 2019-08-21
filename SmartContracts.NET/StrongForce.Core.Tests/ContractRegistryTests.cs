using System;
using System.Collections.Generic;
using StrongForce.Core.Tests.Mocks;
using Xunit;

namespace StrongForce.Core.Tests
{
	public class ContractRegistryTests
	{
		[Fact]
		public void CreateContract_WhenPassedNewContract_AddsItCorrectly()
		{
			var registry = new ContractRegistry();

			Address contractAddress = registry.CreateContract<FavoriteNumberContract>();

			Assert.NotNull(registry.GetContract(contractAddress));
			Assert.Equal(typeof(FavoriteNumberContract), registry.GetContract(contractAddress).GetType());
		}

		[Fact]
		public void CreateContract_WhenPassedNonContractType_ThrowsArgumentException()
		{
			var registry = new ContractRegistry();

			Assert.Throws<ArgumentOutOfRangeException>(() => registry.CreateContract<Action>());
		}

		[Fact]
		public void CreateContract_WhenPassedNull_ThrowsArgumentNullException()
		{
			var registry = new ContractRegistry();

			Assert.Throws<ArgumentOutOfRangeException>(() => registry.CreateContract(null));
		}

		[Fact]
		public void GetContract_WhenPassedNonExistentContract_ReturnsNull()
		{
			var registry = new ContractRegistry();
			Address contractAddress = registry.AddressFactory.Create();

			Assert.Null(registry.GetContract(contractAddress));
		}

		[Fact]
		public void GetContract_WhenPassedNull_ThrowsArgumentNullException()
		{
			var registry = new ContractRegistry();
			Assert.Throws<ArgumentNullException>(() => registry.GetContract(null));
		}

		[Fact]
		public void SendAction_WhenPassedValidAction_SendsActionToCorrectContract()
		{
			var registry = new ContractRegistry();
			Address senderAddress = registry.AddressFactory.Create();
			Address contractAddress = registry.CreateContract<FavoriteNumberContract>(new Dictionary<string, object>()
			{
				{ "User", senderAddress.ToBase64String() },
			});

			registry.SendAction(senderAddress, contractAddress, SetFavoriteNumberAction.Type, new Dictionary<string, object>
			{
				{ SetFavoriteNumberAction.Number, 0 },
			});

			var contract = registry.GetContract(contractAddress) as FavoriteNumberContract;

			Assert.Equal(contract.LastOrigin, senderAddress);
			Assert.Equal(contract.LastSender, senderAddress);
		}

		[Fact]
		public void SendAction_WhenPassedNull_ThrowsArgumentNullException()
		{
			var registry = new ContractRegistry();
			Address address = registry.AddressFactory.Create();
			var actionType = "NotARealActionType";
			var payload = new Dictionary<string, object>();

			Assert.Throws<ArgumentNullException>(() => registry.SendAction(address, (Address[])null, actionType, payload));
			Assert.Throws<ArgumentNullException>(() => registry.SendAction(address, address, null, payload));
			Assert.Throws<ArgumentNullException>(() => registry.SendAction(address, address, actionType, null));
		}

		[Fact]
		public void HandleAction_WhenPassedActionWithNonExistentAddress_ReturnsFalse()
		{
			var registry = new ContractRegistry();
			Address address = registry.AddressFactory.Create();
			var actionType = "NotARealActionType";
			var payload = new Dictionary<string, object>();

			Assert.False(registry.SendAction(address, address, actionType, payload));
		}
	}
}