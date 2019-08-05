using StrongForce.Core.Exceptions;
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
			var creatorAddress = registry.CreateContract(typeof(CreatorContract), new object[] { null });

			var action = new CreateContractAction(creatorAddress, typeof(FavoriteNumberContract));
			registry.HandleAction(creatorAddress, action);

			var lastAddress = ((CreatorContract)registry.GetContract(creatorAddress)).LastCreatedAddress;
			Assert.NotNull(lastAddress);
			Assert.NotNull(registry.GetContract(lastAddress));
		}

		[Fact]
		public void CreateContractAction_WhenConfigured_AllowsForwarding()
		{
			var registry = new ContractRegistry();
			var adminAddress = new Address(new byte[] { 1 });
			var creatorAddress = registry.CreateContract(typeof(CreatorContract), adminAddress);

			var permissionAction = new AddPermissionAction(
				creatorAddress,
				typeof(SetFavoriteNumberAction),
				adminAddress,
				null);

			registry.HandleAction(adminAddress, permissionAction);

			var createAction = new CreateContractAction(creatorAddress, typeof(FavoriteNumberContract));
			registry.HandleAction(adminAddress, createAction);

			var newAddress = ((CreatorContract)registry.GetContract(creatorAddress)).LastCreatedAddress;

			// First, prove that we cannot directly interact with the contract
			var setNumberAction = new SetFavoriteNumberAction(newAddress, 21);

			Assert.Throws<NoPermissionException>(() => registry.HandleAction(adminAddress, setNumberAction));

			// Second, prove that we can interact with the contract through the forwarding permission set up earlier
			var forwardedNumberAction = new ForwardAction(creatorAddress, new SetFavoriteNumberAction(newAddress, 42));

			registry.HandleAction(adminAddress, forwardedNumberAction);

			Assert.Equal(42, ((FavoriteNumberContract)registry.GetContract(newAddress)).Number);
		}
	}
}