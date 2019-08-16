using System.Collections.Generic;
using System.Linq;
using StrongForce.Core.Exceptions;
using StrongForce.Core.Extensions;
using StrongForce.Core.Permissions;
using StrongForce.Core.Tests.Mocks;
using Xunit;

namespace StrongForce.Core.Tests
{
	public class PermittedContractTests
	{
		private ContractRegistry registry;

		public PermittedContractTests()
		{
			this.registry = new ContractRegistry();
		}

		[Fact]
		public void Permissions_WhenContractInitialized_ReturnTrue()
		{
			Address permissionManager = this.registry.AddressFactory.Create();
			var contractAddress = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", permissionManager?.ToBase64String() } });

			Contract contract = this.registry.GetContract(contractAddress);

			Assert.True(contract.CheckPermission(permissionManager, AddPermissionAction.Type, contractAddress));
			Assert.True(contract.CheckPermission(permissionManager, RemovePermissionAction.Type, contractAddress));
		}

		[Fact]
		public void Receive_WhenPassedActionWithNoPermissions_ThrowsNoPermissionException()
		{
			Address permissionManager = this.registry.AddressFactory.Create();
			Address otherAddress = this.registry.AddressFactory.Create();

			var contractAddress = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", permissionManager?.ToBase64String() } });

			Assert.Throws<NoPermissionException>(() =>
			{
				this.registry.SendAction(otherAddress, contractAddress, AddPermissionAction.Type, new Dictionary<string, object>()
				{
					{ AddPermissionAction.PermissionType, SetFavoriteNumberAction.Type },
					{ AddPermissionAction.PermissionSender, contractAddress?.ToBase64String() },
					{ AddPermissionAction.PermissionTarget, contractAddress?.ToBase64String() },
				});
			});
		}

		[Fact]
		public void Receive_WhenPassedSupportedActionWithPermissions_ReturnsTrue()
		{
			Address permissionManager = this.registry.AddressFactory.Create();

			var contractAddress = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", permissionManager?.ToBase64String() } });

			Assert.True(this.registry.SendAction(permissionManager, contractAddress, AddPermissionAction.Type, new Dictionary<string, object>()
			{
				{ AddPermissionAction.PermissionType, SetFavoriteNumberAction.Type },
				{ AddPermissionAction.PermissionSender, contractAddress?.ToBase64String() },
				{ AddPermissionAction.PermissionTarget, contractAddress?.ToBase64String() },
			}));

			Contract contract = this.registry.GetContract(contractAddress);

			Assert.True(contract.CheckPermission(contractAddress, SetFavoriteNumberAction.Type, contractAddress));
		}

		[Fact]
		public void Receive_WhenRemovePermitedAction_ReturnsTrue()
		{
			Address permissionManager = this.registry.AddressFactory.Create();

			var contractAddress = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", permissionManager?.ToBase64String() } });

			Contract contract = this.registry.GetContract(contractAddress);

			Assert.True(this.registry.SendAction(permissionManager, contractAddress, AddPermissionAction.Type, new Dictionary<string, object>()
			{
				{ AddPermissionAction.PermissionType, SetFavoriteNumberAction.Type },
				{ AddPermissionAction.PermissionSender, permissionManager?.ToBase64String() },
				{ AddPermissionAction.PermissionTarget, contractAddress?.ToBase64String() },
			}));

			Assert.True(contract.CheckPermission(permissionManager, SetFavoriteNumberAction.Type, contractAddress));

			Assert.True(this.registry.SendAction(permissionManager, contractAddress, RemovePermissionAction.Type, new Dictionary<string, object>()
			{
				{ RemovePermissionAction.PermissionType, SetFavoriteNumberAction.Type },
				{ RemovePermissionAction.PermissionSender, permissionManager?.ToBase64String() },
				{ RemovePermissionAction.PermissionTarget, contractAddress?.ToBase64String() },
			}));

			Assert.False(contract.CheckPermission(permissionManager, SetFavoriteNumberAction.Type, contractAddress));
		}

		[Fact]
		public void Receive_WhenPassedUnsupportedActionWithPermissions_ReturnsFalse()
		{
			Address permissionManager = this.registry.AddressFactory.Create();

			var contractAddress = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", permissionManager?.ToBase64String() } });

			Assert.Throws<NoPermissionException>(() =>
			{
				this.registry.SendAction(permissionManager, contractAddress, "NotARealActionType", new Dictionary<string, object>());
			});
		}

		[Fact]
		public void AddPermission_WithWildcard_ReturnsTrue()
		{
			Address permissionManager = this.registry.AddressFactory.Create();

			var contractAddress = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", permissionManager?.ToBase64String() } });

			this.registry.SendAction(permissionManager, contractAddress, AddPermissionAction.Type, new Dictionary<string, object>()
			{
				{ AddPermissionAction.PermissionType, SetFavoriteNumberAction.Type },
				{ AddPermissionAction.PermissionSender, null },
				{ AddPermissionAction.PermissionTarget, contractAddress?.ToBase64String() },
			});

			Contract contract = this.registry.GetContract(contractAddress);

			Assert.True(contract.CheckPermission(this.registry.AddressFactory.Create(), SetFavoriteNumberAction.Type, contractAddress));
		}

		[Fact]
		public void ForwardAction_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.registry.AddressFactory.Create();
			var contract1Address = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", permissionManager?.ToBase64String() } });
			var contract2Address = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", contract1Address?.ToBase64String() } });
			int testedNumber = 424;

			this.registry.SendAction(permissionManager, contract1Address, AddPermissionAction.Type, new Dictionary<string, object>()
			{
				{ AddPermissionAction.PermissionType, SetFavoriteNumberAction.Type },
				{ AddPermissionAction.PermissionSender, permissionManager?.ToBase64String() },
				{ AddPermissionAction.PermissionTarget, contract2Address?.ToBase64String() },
			});

			this.registry.SendAction(permissionManager, new Address[] { contract1Address, contract2Address }, SetFavoriteNumberAction.Type, new Dictionary<string, object>()
			{
				{ SetFavoriteNumberAction.Number, testedNumber },
			});

			FavoriteNumberContract contract2 = (FavoriteNumberContract)this.registry.GetContract(contract2Address);
			Assert.Equal(testedNumber, contract2.Number);
		}

		[Fact]
		public void ForwardAction_WithoutIntermediaryPermission_ThrowsNoPermissionException()
		{
			Address permissionManager = this.registry.AddressFactory.Create();
			var contract1Address = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", permissionManager?.ToBase64String() } });
			var contract2Address = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", contract1Address?.ToBase64String() } });
			int testedNumber = 424;

			Assert.Throws<NoPermissionException>(() =>
			{
				this.registry.SendAction(permissionManager, new Address[] { contract1Address, contract2Address }, SetFavoriteNumberAction.Type, new Dictionary<string, object>()
				{
					{ SetFavoriteNumberAction.Number, testedNumber },
				});
			});
		}

		[Fact]
		public void ForwardAction_WithoutFinalPermission_ThrowsNoPermissionException()
		{
			Address permissionManager = this.registry.AddressFactory.Create();
			var contract1Address = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", permissionManager?.ToBase64String() } });
			var contract2Address = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", contract1Address?.ToBase64String() } });
			int testedNumber = 424;

			this.registry.SendAction(permissionManager, contract1Address, AddPermissionAction.Type, new Dictionary<string, object>()
			{
				{ AddPermissionAction.PermissionType, SetFavoriteNumberAction.Type },
				{ AddPermissionAction.PermissionSender, permissionManager?.ToBase64String() },
				{ AddPermissionAction.PermissionTarget, contract2Address?.ToBase64String() },
			});

			this.registry.SendAction(contract1Address, contract2Address, RemovePermissionAction.Type, new Dictionary<string, object>()
			{
				{ RemovePermissionAction.PermissionType, SetFavoriteNumberAction.Type },
				{ RemovePermissionAction.PermissionSender, contract1Address?.ToBase64String() },
				{ RemovePermissionAction.PermissionTarget, contract2Address?.ToBase64String() },
			});

			Assert.Throws<NoPermissionException>(() =>
			{
				this.registry.SendAction(permissionManager, new Address[] { contract1Address, contract2Address }, SetFavoriteNumberAction.Type, new Dictionary<string, object>()
				{
					{ SetFavoriteNumberAction.Number, testedNumber },
				});
			});
		}

		[Fact]
		public void RemovePermissionSender_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.registry.AddressFactory.Create();

			var contractAddress = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", permissionManager?.ToBase64String() } });

			Address permitedAddress = this.registry.AddressFactory.Create();
			string permission = "NotARealActionType";

			this.registry.SendAction(permissionManager, contractAddress, AddPermissionAction.Type, new Dictionary<string, object>()
			{
				{ AddPermissionAction.PermissionType, permission },
				{ AddPermissionAction.PermissionSender, permissionManager?.ToBase64String() },
				{ AddPermissionAction.PermissionTarget, contractAddress?.ToBase64String() },
			});

			this.registry.SendAction(permissionManager, contractAddress, AddPermissionAction.Type, new Dictionary<string, object>()
			{
				{ AddPermissionAction.PermissionType, permission },
				{ AddPermissionAction.PermissionSender, permitedAddress?.ToBase64String() },
				{ AddPermissionAction.PermissionTarget, contractAddress?.ToBase64String() },
			});

			this.registry.SendAction(permissionManager, contractAddress, RemovePermissionAction.Type, new Dictionary<string, object>()
			{
				{ RemovePermissionAction.PermissionType, permission },
				{ RemovePermissionAction.PermissionSender, permissionManager?.ToBase64String() },
				{ RemovePermissionAction.PermissionTarget, contractAddress?.ToBase64String() },
			});

			Contract contract = this.registry.GetContract(contractAddress);
			Assert.True(contract.CheckPermission(permitedAddress, permission, contractAddress));
			Assert.False(contract.CheckPermission(permissionManager, permission, contractAddress));
		}
	}
}