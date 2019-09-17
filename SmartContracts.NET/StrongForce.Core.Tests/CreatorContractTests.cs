using System.Collections.Generic;
using StrongForce.Core.Exceptions;
using StrongForce.Core.Extensions;
using StrongForce.Core.Permissions;
using StrongForce.Core.Tests.Mocks;
using Xunit;

namespace StrongForce.Core.Tests
{
	public class CreatorContractTests
	{
		[Fact]
		public void CreateContractAction_WhenPassedFavoriteNumberContract_CreatesContract()
		{
			var registry = new TestRegistry();
			var permissionManager = registry.AddressFactory.CreateAddress();
			var creatorAddress = registry.CreateContract<CreatorContract>(new Dictionary<string, object>()
			{
				{ "User", permissionManager.ToString() },
			});

			registry.SendMessage(permissionManager, creatorAddress, CreateContractAction.Type, new Dictionary<string, object>()
			{
				{ CreateContractAction.ContractType, typeof(FavoriteNumberContract).ToString() },
			});

			var lastAddress = ((CreatorContract)registry.GetContract(creatorAddress)).LastCreatedAddress;
			Assert.NotNull(lastAddress);
			Assert.NotNull(registry.GetContract(lastAddress));
		}

		[Fact]
		public void CreateContractAction_WhenPassedMessages_SendsMessages()
		{
			var registry = new TestRegistry();
			var targetNumber = 367;
			var permissionManager = registry.AddressFactory.CreateAddress();
			var creatorAddress = registry.CreateContract<CreatorContract>(new Dictionary<string, object>()
			{
				{ "User", permissionManager.ToString() },
			});

			registry.SendMessage(permissionManager, creatorAddress, CreateContractAction.Type, new Dictionary<string, object>()
			{
				{ CreateContractAction.ContractType, typeof(FavoriteNumberContract).ToString() },
				{
					CreateContractAction.Messages,
					new List<object>()
					{
						new Dictionary<string, object>()
						{
							{ CreateContractAction.MessageType, SetFavoriteNumberAction.Type },
							{ CreateContractAction.MessagePayload, new Dictionary<string, object>() { { SetFavoriteNumberAction.Number, targetNumber } } },
						},
					}
				},
			});

			var lastAddress = ((CreatorContract)registry.GetContract(creatorAddress)).LastCreatedAddress;
			Assert.NotNull(lastAddress);
			Assert.NotNull(registry.GetContract(lastAddress));
			Assert.Equal(targetNumber, ((FavoriteNumberContract)registry.GetContract(lastAddress)).Number);
		}

		[Fact]
		public void CreateContractAction_WhenPassedInvalidMessages_Throws()
		{
			var registry = new TestRegistry();
			var permissionManager = registry.AddressFactory.CreateAddress();
			var creatorAddress = registry.CreateContract<CreatorContract>(new Dictionary<string, object>()
			{
				{ "User", permissionManager.ToString() },
			});

			Assert.Throws<NoPermissionException>(() => registry.SendMessage(permissionManager, creatorAddress, CreateContractAction.Type, new Dictionary<string, object>()
			{
				{ CreateContractAction.ContractType, typeof(FavoriteNumberContract).ToString() },
				{
					CreateContractAction.Messages,
					new List<object>()
					{
						new Dictionary<string, object>()
						{
							{ CreateContractAction.MessageType, "NotARealActionType" },
							{ CreateContractAction.MessagePayload, new Dictionary<string, object>() { } },
						},
					}
				},
			}));
		}

		[Fact]
		public void CreateContractAction_WhenConfigured_AllowsForwarding()
		{
			var registry = new TestRegistry();
			var permissionManager = new Address(new byte[] { 1 });
			var creatorAddress = registry.CreateContract<CreatorContract>(new Dictionary<string, object>()
			{
				{ "Admin", permissionManager.ToString() },
				{ "User", permissionManager.ToString() },
			});

			registry.SendMessage(permissionManager, creatorAddress, AddPermissionAction.Type, new Dictionary<string, object>()
			{
				{ AddPermissionAction.PermissionType, SetFavoriteNumberAction.Type },
				{ AddPermissionAction.PermissionSender, permissionManager?.ToString() },
				{ AddPermissionAction.PermissionTarget, null },
			});

			registry.SendMessage(permissionManager, creatorAddress, CreateContractAction.Type, new Dictionary<string, object>()
			{
				{ CreateContractAction.ContractType, typeof(FavoriteNumberContract).ToString() },
			});

			var newAddress = ((CreatorContract)registry.GetContract(creatorAddress)).LastCreatedAddress;

			// First, prove that we cannot directly interact with the contract
			Assert.Throws<NoPermissionException>(() =>
			{
				registry.SendMessage(permissionManager, newAddress, SetFavoriteNumberAction.Type, new Dictionary<string, object>()
				{
					{ SetFavoriteNumberAction.Number, 45 },
				});
			});

			registry.SendMessage(permissionManager, new Address[] { creatorAddress, newAddress }, SetFavoriteNumberAction.Type, new Dictionary<string, object>()
			{
				{ SetFavoriteNumberAction.Number, 42 },
			});

			Assert.Equal(42, ((FavoriteNumberContract)registry.GetContract(newAddress)).Number);
		}
	}
}