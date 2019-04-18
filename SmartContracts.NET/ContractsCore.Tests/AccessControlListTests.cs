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

		[Fact]
		public void AddPermission_WithWildCard_ReturnsTrue()
		{
			var acl = new AccessControlList();
			var permission = new Permission(typeof(Action));
			AnyWildCard anyWildCard = new AnyWildCard();
			Assert.True(acl.AddPermission(anyWildCard, permission, anyWildCard));
		}

		[Fact]
		public void HasPermission_WhenGrantedWithAddressWildCard_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = new Permission(typeof(Action));
			AnyWildCard anyWildCard = new AnyWildCard();
			AddressWildCard addressWildCard = new AddressWildCard { address };
			Assert.True(acl.AddPermission(addressWildCard, permission, anyWildCard));
			Assert.True(acl.HasPermission(address, permission, address));
		}

		[Fact]
		public void HasPermission_WhenGrantedWithAnyWildCard_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = new Permission(typeof(Action));
			AnyWildCard anyWildCard = new AnyWildCard();
			Assert.True(acl.AddPermission(anyWildCard, permission, anyWildCard));
			Assert.True(acl.HasPermission(address, permission, address));
		}

		[Fact]
		public void AddPermissionExecutor_WhenPermissionExists_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			Address newAddress = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = new Permission(typeof(Action));
			AnyWildCard anyWildCard = new AnyWildCard();

			Assert.True(acl.AddPermission(address, permission, anyWildCard));

			Assert.True(acl.AddPermissionExecutor(newAddress, permission, anyWildCard));
			Assert.True(acl.HasPermission(newAddress, permission, anyWildCard));
		}

		[Fact]
		public void AddPermissionNextAddress_WhenPermissionExists_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			Address newAddress = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = new Permission(typeof(Action));
			AnyWildCard anyWildCard = new AnyWildCard();

			Assert.True(acl.AddPermission(anyWildCard, permission, address));
			Assert.False(acl.HasPermission(anyWildCard, permission, newAddress));
			Assert.True(acl.AddPermissionNextAddress(anyWildCard, permission, newAddress));
			Assert.True(acl.HasPermission(anyWildCard, permission, newAddress));
		}

		[Fact]
		public void RemovePermissionExecutor_WhenPermissionExists_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			Address newAddress = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = new Permission(typeof(Action));
			AnyWildCard anyWildCard = new AnyWildCard();
			AddressWildCard addressWildCard = new AddressWildCard { address, newAddress };

			Assert.True(acl.AddPermission(addressWildCard, permission, anyWildCard));
			Assert.True(acl.HasPermission(newAddress, permission, anyWildCard));
			Assert.True(acl.RemovePermissionExecutor(newAddress, permission, anyWildCard));
			Assert.False(acl.HasPermission(newAddress, permission, anyWildCard));
		}

		[Fact]
		public void RemovePermissionNextAddress_WhenRemovingSingleAddress_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			Address newAddress = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = new Permission(typeof(Action));
			AnyWildCard anyWildCard = new AnyWildCard();
			AddressWildCard addressWildCard = new AddressWildCard { address, newAddress };

			Assert.True(acl.AddPermission(anyWildCard, permission, addressWildCard));
			Assert.True(acl.HasPermission(anyWildCard, permission, addressWildCard));
			Assert.True(acl.RemovePermissionNextAddress(anyWildCard, permission, newAddress));
			Assert.False(acl.HasPermission(anyWildCard, permission, newAddress));
			Assert.False(acl.HasPermission(anyWildCard, permission, addressWildCard));
		}

		[Fact]
		public void RemovePermissionNextAddress_WhenRemovingWildCard_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			Address newAddress = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = new Permission(typeof(Action));
			AnyWildCard anyWildCard = new AnyWildCard();
			AddressWildCard addressWildCard = new AddressWildCard { address, newAddress };

			Assert.True(acl.AddPermission(anyWildCard, permission, addressWildCard));
			Assert.True(acl.HasPermission(anyWildCard, permission, addressWildCard));
			Assert.True(acl.RemovePermissionNextAddress(anyWildCard, permission, addressWildCard));
			Assert.False(acl.HasPermission(anyWildCard, permission, newAddress));
			Assert.False(acl.HasPermission(anyWildCard, permission, addressWildCard));
		}
	}
}