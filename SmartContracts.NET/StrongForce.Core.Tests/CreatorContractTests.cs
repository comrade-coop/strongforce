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
			var registry = new ContractRegistry();
			var permissionManager = registry.AddressFactory.Create();
			var creatorAddress = registry.CreateContract<CreatorContract>(new Dictionary<string, object>()
			{
				{ "User", permissionManager.ToBase64String() },
			});

			registry.SendAction(permissionManager, creatorAddress, CreateContractAction.Type, new Dictionary<string, object>()
			{
				{ CreateContractAction.ContractType, typeof(FavoriteNumberContract).ToString() },
			});

			var lastAddress = ((CreatorContract)registry.GetContract(creatorAddress)).LastCreatedAddress;
			Assert.NotNull(lastAddress);
			Assert.NotNull(registry.GetContract(lastAddress));
		}

		[Fact]
		public void CreateContractAction_WhenConfigured_AllowsForwarding()
		{
			var registry = new ContractRegistry();
			var permissionManager = new Address(new byte[] { 1 });
			var creatorAddress = registry.CreateContract<CreatorContract>(new Dictionary<string, object>()
			{
				{ "Admin", permissionManager.ToBase64String() },
				{ "User", permissionManager.ToBase64String() },
			});

			registry.SendAction(permissionManager, creatorAddress, AddPermissionAction.Type, new Dictionary<string, object>()
			{
				{ AddPermissionAction.PermissionType, SetFavoriteNumberAction.Type },
				{ AddPermissionAction.PermissionSender, permissionManager?.ToBase64String() },
				{ AddPermissionAction.PermissionTarget, null },
			});

			registry.SendAction(permissionManager, creatorAddress, CreateContractAction.Type, new Dictionary<string, object>()
			{
				{ CreateContractAction.ContractType, typeof(FavoriteNumberContract).ToString() },
			});

			var newAddress = ((CreatorContract)registry.GetContract(creatorAddress)).LastCreatedAddress;

			// First, prove that we cannot directly interact with the contract
			Assert.Throws<NoPermissionException>(() =>
			{
				registry.SendAction(permissionManager, newAddress, SetFavoriteNumberAction.Type, new Dictionary<string, object>()
				{
					{ SetFavoriteNumberAction.Number, 45 },
				});
			});

			registry.SendAction(permissionManager, new Address[] { creatorAddress, newAddress }, SetFavoriteNumberAction.Type, new Dictionary<string, object>()
			{
				{ SetFavoriteNumberAction.Number, 42 },
			});

			Assert.Equal(42, ((FavoriteNumberContract)registry.GetContract(newAddress)).Number);
		}
	}
}