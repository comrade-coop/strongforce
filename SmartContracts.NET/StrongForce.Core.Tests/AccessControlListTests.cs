using System.Collections.Generic;
using StrongForce.Core.Permissions;
using Xunit;

namespace StrongForce.Core.Tests
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
			var permission = typeof(Action);

			Assert.True(acl.AddPermission(address, permission, address));
		}

		[Fact]
		public void AddPermission_WhenDuplicatePermissionsArePassed_ReturnsFalse()
		{
			Address address = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = typeof(Action);

			acl.AddPermission(address, permission, address);
			Assert.False(acl.AddPermission(address, permission, address));
		}

		[Fact]
		public void AddPermission_WhenNullIsPassed_ReturnsTrue()
		{
			var acl = new AccessControlList();
			Assert.True(acl.AddPermission(null, null, null));
		}

		[Fact]
		public void HasPermission_WhenPermissionHasBeenAdded_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = typeof(Action);

			acl.AddPermission(address, permission, address);
			Assert.True(acl.HasPermission(address, permission, address));
		}

		[Fact]
		public void HasPermission_WhenPermissionHasNotBeenAdded_ReturnsFalse()
		{
			Address address = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = typeof(Action);

			Assert.False(acl.HasPermission(address, permission, address));
		}

		[Fact]
		public void RemovePermission_WhenPermissionHasBeenAdded_RemovesPermission()
		{
			Address address = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = typeof(Action);

			acl.AddPermission(address, permission, address);
			Assert.True(acl.RemovePermission(address, permission, address));
			Assert.False(acl.HasPermission(address, permission, address));
		}

		[Fact]
		public void RemovePermission_WhenPermissionHasNotBeenAdded_ReturnsFalse()
		{
			Address address = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = typeof(Action);

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
			var permission = typeof(Action);

			Assert.True(acl.AddPermission(null, permission, null));
		}

		[Fact]
		public void HasPermission_WhenGrantedWithHashSet_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = typeof(Action);

			Assert.True(acl.AddPermission(address, permission, null));
			Assert.True(acl.HasPermission(address, permission, address));
		}

		[Fact]
		public void HasPermission_WhenGrantedWithAnyWildCard_ReturnsTrue()
		{
			Address address = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = typeof(Action);

			Assert.True(acl.AddPermission(null, permission, null));
			Assert.True(acl.HasPermission(address, permission, address));
		}

		[Fact]
		public void UpdatePermissionSender_WhenPermissionExists_ReturnsTrue()
		{
			var address = this.addressFactory.Create();
			var newAddress = this.addressFactory.Create();
			var acl = new AccessControlList();
			var permission = typeof(Action);

			Assert.True(acl.AddPermission(address, permission, null));

			Assert.True(acl.AddPermission(newAddress, permission, null));
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
			var permission = typeof(Action);

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

			var acl = new AccessControlList();
			var permission = typeof(Action);

			Assert.True(acl.AddPermission(address, permission, null));
			Assert.True(acl.AddPermission(newAddress, permission, null));

			Assert.True(acl.RemovePermission(newAddress, permission, null));
			Assert.True(acl.HasPermission(address, permission, this.addressFactory.Create()));
			Assert.False(acl.HasPermission(newAddress, permission, this.addressFactory.Create()));
		}

		private HashSet<Address> GenerateWildCard(Address address)
		{
			return new HashSet<Address> { address };
		}
	}
}