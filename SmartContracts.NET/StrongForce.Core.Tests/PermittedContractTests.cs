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
		private TestRegistry registry;

		public PermittedContractTests()
		{
			this.registry = new TestRegistry();
		}

		[Fact]
		public void Permissions_WhenContractInitialized_ReturnTrue()
		{
			Address permissionManager = this.registry.AddressFactory.CreateAddress();
			var contractAddress = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", permissionManager?.ToString() } });

			var contract = this.registry.GetContract<Contract>(contractAddress);

			Assert.True(contract.CheckPermission(permissionManager, AddPermissionAction.Type, contractAddress));
			Assert.True(contract.CheckPermission(permissionManager, RemovePermissionAction.Type, contractAddress));
		}

		[Fact]
		public void Receive_WhenPassedActionWithNoPermissions_ThrowsNoPermissionException()
		{
			Address permissionManager = this.registry.AddressFactory.CreateAddress();
			Address otherAddress = this.registry.AddressFactory.CreateAddress();

			var contractAddress = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", permissionManager?.ToString() } });

			Assert.Throws<NoPermissionException>(() =>
			{
				this.registry.SendMessage(otherAddress, contractAddress, AddPermissionAction.Type, new Dictionary<string, object>()
				{
					{ AddPermissionAction.PermissionType, SetFavoriteNumberAction.Type },
					{ AddPermissionAction.PermissionSender, contractAddress?.ToString() },
					{ AddPermissionAction.PermissionTarget, contractAddress?.ToString() },
				});
			});
		}

		[Fact]
		public void Receive_WhenPassedSupportedActionWithPermissions_ReturnsTrue()
		{
			Address permissionManager = this.registry.AddressFactory.CreateAddress();

			var contractAddress = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", permissionManager?.ToString() } });

			this.registry.SendMessage(permissionManager, contractAddress, AddPermissionAction.Type, new Dictionary<string, object>()
			{
				{ AddPermissionAction.PermissionType, SetFavoriteNumberAction.Type },
				{ AddPermissionAction.PermissionSender, contractAddress?.ToString() },
				{ AddPermissionAction.PermissionTarget, contractAddress?.ToString() },
			});

			var contract = this.registry.GetContract<Contract>(contractAddress);

			Assert.True(contract.CheckPermission(contractAddress, SetFavoriteNumberAction.Type, contractAddress));
		}

		[Fact]
		public void Receive_WhenRemovePermitedAction_ReturnsTrue()
		{
			Address permissionManager = this.registry.AddressFactory.CreateAddress();

			var contractAddress = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", permissionManager?.ToString() } });

			this.registry.SendMessage(permissionManager, contractAddress, AddPermissionAction.Type, new Dictionary<string, object>()
			{
				{ AddPermissionAction.PermissionType, SetFavoriteNumberAction.Type },
				{ AddPermissionAction.PermissionSender, permissionManager?.ToString() },
				{ AddPermissionAction.PermissionTarget, contractAddress?.ToString() },
			});

			Assert.True(this.registry.GetContract<Contract>(contractAddress).CheckPermission(permissionManager, SetFavoriteNumberAction.Type, contractAddress));

			this.registry.SendMessage(permissionManager, contractAddress, RemovePermissionAction.Type, new Dictionary<string, object>()
			{
				{ RemovePermissionAction.PermissionType, SetFavoriteNumberAction.Type },
				{ RemovePermissionAction.PermissionSender, permissionManager?.ToString() },
				{ RemovePermissionAction.PermissionTarget, contractAddress?.ToString() },
			});

			Assert.False(this.registry.GetContract<Contract>(contractAddress).CheckPermission(permissionManager, SetFavoriteNumberAction.Type, contractAddress));
		}

		[Fact]
		public void Receive_WhenPassedUnsupportedActionWithPermissions_ReturnsFalse()
		{
			Address permissionManager = this.registry.AddressFactory.CreateAddress();

			var contractAddress = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", permissionManager?.ToString() } });

			Assert.Throws<NoPermissionException>(() =>
			{
				this.registry.SendMessage(permissionManager, contractAddress, "NotARealActionType", new Dictionary<string, object>());
			});
		}

		[Fact]
		public void AddPermission_WithWildcard_ReturnsTrue()
		{
			Address permissionManager = this.registry.AddressFactory.CreateAddress();

			var contractAddress = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", permissionManager.ToString() } });

			this.registry.SendMessage(permissionManager, contractAddress, AddPermissionAction.Type, new Dictionary<string, object>()
			{
				{ AddPermissionAction.PermissionType, SetFavoriteNumberAction.Type },
				{ AddPermissionAction.PermissionSender, null },
				{ AddPermissionAction.PermissionTarget, contractAddress?.ToString() },
			});

			var contract = this.registry.GetContract<Contract>(contractAddress);

			Assert.True(contract.CheckPermission(this.registry.AddressFactory.CreateAddress(), SetFavoriteNumberAction.Type, contractAddress));
		}

		[Fact]
		public void ForwardAction_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.registry.AddressFactory.CreateAddress();
			var contract1Address = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", permissionManager.ToString() } });
			var contract2Address = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "User", contract1Address.ToString() } });
			int testedNumber = 424;

			this.registry.SendMessage(permissionManager, contract1Address, AddPermissionAction.Type, new Dictionary<string, object>()
			{
				{ AddPermissionAction.PermissionType, SetFavoriteNumberAction.Type },
				{ AddPermissionAction.PermissionSender, permissionManager?.ToString() },
				{ AddPermissionAction.PermissionTarget, contract2Address?.ToString() },
			});

			this.registry.SendMessage(permissionManager, new Address[] { contract1Address, contract2Address }, SetFavoriteNumberAction.Type, new Dictionary<string, object>()
			{
				{ SetFavoriteNumberAction.Number, testedNumber },
			});

			var contract2 = this.registry.GetContract<FavoriteNumberContract>(contract2Address);
			Assert.Equal(testedNumber, contract2.Number);
		}

		[Fact]
		public void ForwardAction_WithoutIntermediaryPermission_ThrowsNoPermissionException()
		{
			Address permissionManager = this.registry.AddressFactory.CreateAddress();
			var contract1Address = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", permissionManager.ToString() } });
			var contract2Address = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "User", contract1Address.ToString() } });
			int testedNumber = 424;

			Assert.Throws<NoPermissionException>(() =>
			{
				this.registry.SendMessage(permissionManager, new Address[] { contract1Address, contract2Address }, SetFavoriteNumberAction.Type, new Dictionary<string, object>()
				{
					{ SetFavoriteNumberAction.Number, testedNumber },
				});
			});
		}

		[Fact]
		public void ForwardAction_WithoutFinalPermission_ThrowsNoPermissionException()
		{
			Address permissionManager = this.registry.AddressFactory.CreateAddress();
			var contract1Address = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", permissionManager.ToString() } });
			var contract2Address = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", permissionManager.ToString() }, { "User", contract1Address.ToString() } });
			int testedNumber = 424;

			this.registry.SendMessage(permissionManager, contract1Address, AddPermissionAction.Type, new Dictionary<string, object>()
			{
				{ AddPermissionAction.PermissionType, SetFavoriteNumberAction.Type },
				{ AddPermissionAction.PermissionSender, permissionManager?.ToString() },
				{ AddPermissionAction.PermissionTarget, contract2Address?.ToString() },
			});

			this.registry.SendMessage(permissionManager, contract2Address, RemovePermissionAction.Type, new Dictionary<string, object>()
			{
				{ RemovePermissionAction.PermissionType, SetFavoriteNumberAction.Type },
				{ RemovePermissionAction.PermissionSender, contract1Address?.ToString() },
				{ RemovePermissionAction.PermissionTarget, contract2Address?.ToString() },
			});

			Assert.Throws<NoPermissionException>(() =>
			{
				this.registry.SendMessage(permissionManager, new Address[] { contract1Address, contract2Address }, SetFavoriteNumberAction.Type, new Dictionary<string, object>()
				{
					{ SetFavoriteNumberAction.Number, testedNumber },
				});
			});
		}

		[Fact]
		public void RemovePermissionSender_WhenPermissionExists_ReturnsTrue()
		{
			Address permissionManager = this.registry.AddressFactory.CreateAddress();

			var contractAddress = this.registry.CreateContract<FavoriteNumberContract>(
				new Dictionary<string, object>() { { "Admin", permissionManager?.ToString() } });

			Address permitedAddress = this.registry.AddressFactory.CreateAddress();
			string permission = "NotARealActionType";

			this.registry.SendMessage(permissionManager, contractAddress, AddPermissionAction.Type, new Dictionary<string, object>()
			{
				{ AddPermissionAction.PermissionType, permission },
				{ AddPermissionAction.PermissionSender, permissionManager?.ToString() },
				{ AddPermissionAction.PermissionTarget, contractAddress?.ToString() },
			});

			this.registry.SendMessage(permissionManager, contractAddress, AddPermissionAction.Type, new Dictionary<string, object>()
			{
				{ AddPermissionAction.PermissionType, permission },
				{ AddPermissionAction.PermissionSender, permitedAddress?.ToString() },
				{ AddPermissionAction.PermissionTarget, contractAddress?.ToString() },
			});

			this.registry.SendMessage(permissionManager, contractAddress, RemovePermissionAction.Type, new Dictionary<string, object>()
			{
				{ RemovePermissionAction.PermissionType, permission },
				{ RemovePermissionAction.PermissionSender, permissionManager?.ToString() },
				{ RemovePermissionAction.PermissionTarget, contractAddress?.ToString() },
			});

			var contract = this.registry.GetContract<Contract>(contractAddress);
			Assert.True(contract.CheckPermission(permitedAddress, permission, contractAddress));
			Assert.False(contract.CheckPermission(permissionManager, permission, contractAddress));
		}
	}
}