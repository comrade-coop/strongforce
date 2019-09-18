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
			var registry = new TestRegistry();

			Address contractAddress = registry.CreateContract<FavoriteNumberContract>();

			Assert.NotNull(registry.GetContract(contractAddress));
			Assert.Equal(typeof(FavoriteNumberContract), registry.GetContract(contractAddress).GetType());
		}

		[Fact]
		public void CreateContract_WhenPassedNonContractType_ThrowsArgumentException()
		{
			var registry = new TestRegistry();

			Assert.Throws<ArgumentOutOfRangeException>(() => registry.CreateContract<Message>());
		}

		[Fact]
		public void SendMessage_WhenPassedValidAction_SendsActionToCorrectContract()
		{
			var registry = new TestRegistry();
			Address senderAddress = registry.AddressFactory.Create();
			Address contractAddress = registry.CreateContract<FavoriteNumberContract>(new Dictionary<string, object>()
			{
				{ "User", senderAddress.ToString() },
			});

			registry.SendMessage(senderAddress, contractAddress, SetFavoriteNumberAction.Type, new Dictionary<string, object>
			{
				{ SetFavoriteNumberAction.Number, 0 },
			});

			var contract = registry.GetContract(contractAddress) as FavoriteNumberContract;

			Assert.Equal(contract.LastOrigin, senderAddress);
			Assert.Equal(contract.LastSender, senderAddress);
		}

		[Fact]
		public void SendMessage_WhenPassedNull_ThrowsArgumentNullException()
		{
			var registry = new TestRegistry();
			Address address = registry.AddressFactory.Create();
			var actionType = "NotARealActionType";
			var payload = new Dictionary<string, object>();

			Assert.Throws<ArgumentNullException>(() => registry.SendMessage(address, (Address[])null, actionType, payload));
			Assert.Throws<ArgumentNullException>(() => registry.SendMessage(address, address, null, payload));
			Assert.Throws<ArgumentNullException>(() => registry.SendMessage(address, address, actionType, null));
		}

		[Fact]
		public void SendMessage_WhenCaughtByContract_ThrowsInvalidOperationException()
		{
			var targetNumber = 48;

			var registry = new TestRegistry();
			Address address = registry.AddressFactory.Create();
			Address buggedContractAddress = registry.CreateContract<CatchingContract>();
			Address targetContractAddress = registry.CreateContract<FavoriteNumberContract>(new Dictionary<string, object>()
			{
				{ "User", buggedContractAddress.ToString() },
			});
			var actionType = "SetFavoriteNumber";
			var invalidPayload = new Dictionary<string, object>() { { "Number", FavoriteNumberContract.InvalidNumber } };
			var validPayload = new Dictionary<string, object>() { { "Number", targetNumber } };

			Assert.Throws<InvalidOperationException>(() => registry.SendMessage(address, new Address[] { buggedContractAddress, targetContractAddress }, actionType, invalidPayload));

			Assert.NotEqual(registry.GetContract<FavoriteNumberContract>(targetContractAddress).Number, FavoriteNumberContract.InvalidNumber);

			registry.SendMessage(address, new Address[] { buggedContractAddress, targetContractAddress }, actionType, validPayload);

			Assert.Equal(registry.GetContract<FavoriteNumberContract>(targetContractAddress).Number, targetNumber);
		}
	}
}