using System.Collections.Generic;
using StrongForce.Core.Permissions;
using StrongForce.Core.Tests.Mocks;
using Xunit;

namespace StrongForce.Core.Tests
{
	public class AccessControlListTests
	{
		private readonly BaseAddressFactory addressFactory;

		public AccessControlListTests()
		{
			this.addressFactory = new RandomAddressFactory();
		}

		[Fact]
		public void AddPermission_WhenNewOnesArePassed_ReturnsTrue()
		{
			Address address = this.addressFactory.CreateAddress();
			var acl = new AccessControlList();

			Assert.True(acl.AddPermission(address, SetFavoriteNumberAction.Type, address));
		}

		[Fact]
		public void AddPermission_WhenDuplicatePermissionsArePassed_ReturnsFalse()
		{
			Address address = this.addressFactory.CreateAddress();
			var acl = new AccessControlList();

			acl.AddPermission(address, SetFavoriteNumberAction.Type, address);
			Assert.False(acl.AddPermission(address, SetFavoriteNumberAction.Type, address));
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
			Address address = this.addressFactory.CreateAddress();
			var acl = new AccessControlList();

			acl.AddPermission(address, SetFavoriteNumberAction.Type, address);
			Assert.True(acl.HasPermission(address, SetFavoriteNumberAction.Type, address));
		}

		[Fact]
		public void HasPermission_WhenPermissionHasNotBeenAdded_ReturnsFalse()
		{
			Address address = this.addressFactory.CreateAddress();
			var acl = new AccessControlList();

			Assert.False(acl.HasPermission(address, SetFavoriteNumberAction.Type, address));
		}

		[Fact]
		public void RemovePermission_WhenPermissionHasBeenAdded_RemovesPermission()
		{
			Address address = this.addressFactory.CreateAddress();
			var acl = new AccessControlList();

			acl.AddPermission(address, SetFavoriteNumberAction.Type, address);
			Assert.True(acl.RemovePermission(address, SetFavoriteNumberAction.Type, address));
			Assert.False(acl.HasPermission(address, SetFavoriteNumberAction.Type, address));
		}

		[Fact]
		public void RemovePermission_WhenPermissionHasNotBeenAdded_ReturnsFalse()
		{
			Address address = this.addressFactory.CreateAddress();
			var acl = new AccessControlList();

			Assert.False(acl.RemovePermission(address, SetFavoriteNumberAction.Type, address));
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

			Assert.True(acl.AddPermission(null, SetFavoriteNumberAction.Type, null));
		}

		[Fact]
		public void HasPermission_WhenGrantedWithHashSet_ReturnsTrue()
		{
			Address address = this.addressFactory.CreateAddress();
			var acl = new AccessControlList();

			Assert.True(acl.AddPermission(address, SetFavoriteNumberAction.Type, null));
			Assert.True(acl.HasPermission(address, SetFavoriteNumberAction.Type, address));
		}

		[Fact]
		public void HasPermission_WhenGrantedWithAnyWildCard_ReturnsTrue()
		{
			Address address = this.addressFactory.CreateAddress();
			var acl = new AccessControlList();

			Assert.True(acl.AddPermission(null, SetFavoriteNumberAction.Type, null));
			Assert.True(acl.HasPermission(address, SetFavoriteNumberAction.Type, address));
		}

		[Fact]
		public void UpdatePermissionSender_WhenPermissionExists_ReturnsTrue()
		{
			var address = this.addressFactory.CreateAddress();
			var newAddress = this.addressFactory.CreateAddress();
			var acl = new AccessControlList();

			Assert.True(acl.AddPermission(address, SetFavoriteNumberAction.Type, null));

			Assert.True(acl.AddPermission(newAddress, SetFavoriteNumberAction.Type, null));
			Assert.True(acl.HasPermission(address, SetFavoriteNumberAction.Type, this.addressFactory.CreateAddress()));
			Assert.True(acl.HasPermission(newAddress, SetFavoriteNumberAction.Type, this.addressFactory.CreateAddress()));
		}

		/*[Fact]
		public void AddPermissionReceiver_WhenPermissionExists_ReturnsTrue()
		{
			var address = this.addressFactory.CreateAddress();
			var newAddress = this.addressFactory.CreateAddress();
			var addressCard = this.GenerateWildCard(address);
			var newAddressCard = this.GenerateWildCard(newAddress);
			newAddressCard.Add(address);
			var acl = new AccessControlList();

			Assert.True(acl.AddPermission(this.anyWildCard, SetFavoriteNumberAction.Type, addressCard));

			Assert.True(acl.UpdatePermission(this.anyWildCard, SetFavoriteNumberAction.Type, this.anyWildCard, newAddressCard));
			Assert.True(acl.HasPermission(this.addressFactory.CreateAddress(), SetFavoriteNumberAction.Type, address));
			Assert.True(acl.HasPermission(this.addressFactory.CreateAddress(), SetFavoriteNumberAction.Type, newAddress));
		}*/

		[Fact]
		public void RemovePermissionSender_WhenPermissionExists_ReturnsTrue()
		{
			var address = this.addressFactory.CreateAddress();
			var newAddress = this.addressFactory.CreateAddress();

			var acl = new AccessControlList();

			Assert.True(acl.AddPermission(address, SetFavoriteNumberAction.Type, null));
			Assert.True(acl.AddPermission(newAddress, SetFavoriteNumberAction.Type, null));

			Assert.True(acl.RemovePermission(newAddress, SetFavoriteNumberAction.Type, null));
			Assert.True(acl.HasPermission(address, SetFavoriteNumberAction.Type, this.addressFactory.CreateAddress()));
			Assert.False(acl.HasPermission(newAddress, SetFavoriteNumberAction.Type, this.addressFactory.CreateAddress()));
		}

		private HashSet<Address> GenerateWildCard(Address address)
		{
			return new HashSet<Address> { address };
		}
	}
}