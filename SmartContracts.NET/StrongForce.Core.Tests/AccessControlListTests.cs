using StrongForce.Core.Permissions;
using System.Collections.Generic;
using Xunit;

namespace StrongForce.Core.Tests
{
	public class AccessControlListTests
	{
		private readonly IAddressFactory addressFactory;
		private HashSet<Address> anyWildCard;
		private Address anyAddress;

		public AccessControlListTests()
		{
			this.addressFactory = new RandomAddressFactory();
			this.anyAddress = Address.Null();
			this.anyWildCard = new HashSet<Address>() { this.anyAddress };
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
			Assert.False(acl.AddPermission(new HashSet<Address>(), null, null));
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
			Assert.True(acl.RemovePermission(permission, address));
			Assert.False(acl.HasPermission(address, permission, address));
		}

		[Fact]
		public void RemovePermission_WhenPermissionHasNotBeenAdded_ReturnsFalse()
		{
			Address address = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = new Permission(typeof(Action));

			Assert.False(acl.RemovePermission(permission, address));
		}

		[Fact]
		public void RemovePermission_WhenNullIsPassed_ReturnsFalse()
		{
			var acl = new AccessControlList();
			Assert.False(acl.RemovePermission(null, null));
		}

		[Fact]
		public void AddPermission_WithWildCard_ReturnsTrue()
		{
			var acl = new AccessControlList();
			var permission = new Permission(typeof(Action));

			Assert.True(acl.AddPermission(this.anyWildCard, permission, this.anyAddress));
		}

		[Fact]
		public void HasPermission_WhenGrantedWithHashSet_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = new Permission(typeof(Action));

			var addressSet = new HashSet<Address> { address };
			Assert.True(acl.AddPermission(addressSet, permission, this.anyAddress));
			Assert.True(acl.HasPermission(address, permission, address));
		}

		[Fact]
		public void HasPermission_WhenGrantedWithAnyWildCard_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = new Permission(typeof(Action));

			Assert.True(acl.AddPermission(this.anyWildCard, permission, this.anyAddress));
			Assert.True(acl.HasPermission(address, permission, address));
		}

		[Fact]
		public void UpdatePermissionSender_WhenPermissionExists_ReturnsTrue()
		{
			var address = this.addressFactory.Create();
			var newAddress = this.addressFactory.Create();
			var addressCard = this.GenerateWildCard(address);
			var newAddressCard = this.GenerateWildCard(newAddress);
			newAddressCard.Add(address);
			var acl = new AccessControlList();
			var permission = new Permission(typeof(Action));

			Assert.True(acl.AddPermission(addressCard, permission, this.anyAddress));

			Assert.True(acl.AddPermission(newAddressCard, permission, this.anyAddress));
			Assert.True(acl.HasPermission(address, permission, this.addressFactory.Create()));
			Assert.True(acl.HasPermission(newAddress, permission, this.addressFactory.Create()));
		}

		/*[Fact]
		public void AddPermissionReceiver_WhenPermissionExists_ReturnsTrue()
		{
			var address = this.addressFactory.Create();
			var newAddress = this.addressFactory.Create();
			var addressCard = this.GenerateWildCard(address);
			var newAddressCard = this.GenerateWildCard(newAddress);
			newAddressCard.Add(address);
			var acl = new AccessControlList();
			var permission = new Permission(typeof(Action));

			Assert.True(acl.AddPermission(this.anyWildCard, permission, addressCard));

			Assert.True(acl.UpdatePermission(this.anyWildCard, permission, this.anyWildCard, newAddressCard));
			Assert.True(acl.HasPermission(this.addressFactory.Create(), permission, address));
			Assert.True(acl.HasPermission(this.addressFactory.Create(), permission, newAddress));
		}*/

		[Fact]
		public void RemovePermissionSender_WhenPermissionExists_ReturnsTrue()
		{
			var address = this.addressFactory.Create();
			var newAddress = this.addressFactory.Create();
			var addressCard = this.GenerateWildCard(address);
			var newAddressCard = this.GenerateWildCard(newAddress);
			addressCard.Add(newAddress);
			var acl = new AccessControlList();
			var permission = new Permission(typeof(Action));

			Assert.True(acl.AddPermission(addressCard, permission, this.anyAddress));

			Assert.True(acl.RemovePermittedAddress(newAddressCard, permission, this.anyAddress));
			Assert.True(acl.HasPermission(address, permission, this.addressFactory.Create()));
			Assert.False(acl.HasPermission(newAddress, permission, this.addressFactory.Create()));
		}

		private HashSet<Address> GenerateWildCard(Address address)
		{
			return new HashSet<Address> { address };
		}
	}
}