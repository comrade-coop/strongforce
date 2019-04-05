using ContractsCore.Actions;
using ContractsCore.Permissions;
using Xunit;

namespace ContractsCore.Tests
{
	public class AccessControlListTests
	{
		private readonly IAddressFactory addressFactory;

		public AccessControlListTests()
		{
			this.addressFactory = new RandomAddressFactory();
		}

		[Fact]
		public void AddPermission_WhenNewOnesArePassed_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = new Permission(typeof(Action));

			Assert.True(acl.AddPermission(address, permission, address));
		}

		[Fact]
		public void AddPermission_WhenDuplicatePermissionsArePassed_ReturnsFalse()
		{
			Address address = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = new Permission(typeof(Action));

			acl.AddPermission(address, permission, address);
			Assert.False(acl.AddPermission(address, permission, address));
		}

		[Fact]
		public void AddPermission_WhenNullIsPassed_ReturnsFalse()
		{
			var acl = new AccessControlList();
			Assert.False(acl.AddPermission(null, null, null));
		}

		[Fact]
		public void HasPermission_WhenPermissionHasBeenAdded_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = new Permission(typeof(Action));

			acl.AddPermission(address, permission, address);
			Assert.True(acl.HasPermission(address, permission, address));
		}

		[Fact]
		public void HasPermission_WhenPermissionHasNotBeenAdded_ReturnsFalse()
		{
			Address address = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = new Permission(typeof(Action));

			Assert.False(acl.HasPermission(address, permission, address));
		}

		[Fact]
		public void RemovePermission_WhenPermissionHasBeenAdded_RemovesPermission()
		{
			Address address = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = new Permission(typeof(Action));

			acl.AddPermission(address, permission, address);
			Assert.True(acl.RemovePermission(address, permission, address));
			Assert.False(acl.HasPermission(address, permission, address));
		}

		[Fact]
		public void RemovePermission_WhenPermissionHasNotBeenAdded_ReturnsFalse()
		{
			Address address = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = new Permission(typeof(Action));

			Assert.False(acl.RemovePermission(address, permission, address));
		}

		[Fact]
		public void RemovePermission_WhenNullIsPassed_ReturnsFalse()
		{
			var acl = new AccessControlList();
			Assert.False(acl.RemovePermission(null, null, null));
		}
	}
}