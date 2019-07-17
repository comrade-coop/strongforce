using System;
using System.Collections.Generic;
using System.Linq;

namespace StrongForce.Core.Permissions
{
	public class AccessControlList
	{
		public IDictionary<Permission, IDictionary<HashSet<Address>, HashSet<Address>>> PermissionsoToSendersToReceivers { get; set; }

		public Address AnyAddressPermitted { get; set; }

		public AccessControlList(Address anyAddressPermitted, IDictionary<Permission, IDictionary<HashSet<Address>, HashSet<Address>>> initialWildCards)
		{
			this.AnyAddressPermitted = anyAddressPermitted;
			this.PermissionsoToSendersToReceivers = initialWildCards;
		}

		public AccessControlList(Address anyAddressPermitted)
			: this(anyAddressPermitted, new SortedDictionary<Permission, IDictionary<HashSet<Address>, HashSet<Address>>>())
		{
		}

		public List<Address> GetPermittedAddresses(Permission permission, Address target)
		{
			IDictionary<HashSet<Address>, HashSet<Address>> sendersToReceivers = this.PermissionsoToSendersToReceivers[permission];
			List<Address> members = new List<Address>();
			foreach (var pair in sendersToReceivers)
			{
				if (pair.Value.Contains(target))
				{
					members = members.Union(pair.Key).ToList();
				}
			}

			return members;
		}

		public bool HasPermission(Address sender, Permission permission, Address receiver)
		{
			if (permission == null || !this.PermissionsoToSendersToReceivers.ContainsKey(permission))
			{
				return false;
			}

			var permissionCheck =
				from pair in this.PermissionsoToSendersToReceivers[permission]
				where (pair.Key.Contains(sender) && pair.Value.Contains(receiver))
				|| (pair.Key.Contains(this.AnyAddressPermitted) && pair.Value.Contains(receiver))
				|| (pair.Key.Contains(this.AnyAddressPermitted) && pair.Value.Contains(this.AnyAddressPermitted))
				|| (pair.Key.Contains(sender) && pair.Value.Contains(this.AnyAddressPermitted))
				select pair;

			return permissionCheck.Count() > 0;
		}

		public bool AddPermission(HashSet<Address> sender, Permission permission, HashSet<Address> receiver)
		{
			if (permission == null)
			{
				return false;
			}

			if (!this.PermissionsoToSendersToReceivers.ContainsKey(permission))
			{
				this.PermissionsoToSendersToReceivers[permission] =
					new SortedDictionary<HashSet<Address>, HashSet<Address>>(new AddressHashSetCompararer())
					{
						{ sender, receiver },
					};
				return true;
			}
			else
			{
				if (this.PermissionsoToSendersToReceivers[permission].ContainsKey(sender))
				{
					HashSet<Address> old = new HashSet<Address>(this.PermissionsoToSendersToReceivers[permission][sender]);
					this.PermissionsoToSendersToReceivers[permission][sender].UnionWith(receiver);
					return !old.SetEquals(this.PermissionsoToSendersToReceivers[permission][sender]);
				}
				else
				{
					this.PermissionsoToSendersToReceivers[permission].Add(sender, receiver);
				}
			}

			return true;
		}

		public bool AddPermission(Address sender, Permission permission, Address receiver)
		{
			if (permission == null)
			{
				return false;
			}

			var senderCard = new HashSet<Address> { sender };
			var receiverCard = new HashSet<Address> { receiver };

			return this.AddPermission(senderCard, permission, receiverCard);
		}

		public bool UpdatePermission(HashSet<Address> oldSender, Permission permission, HashSet<Address> newSender, HashSet<Address> newReceiver)
		{
			if (permission == null
				|| !this.PermissionsoToSendersToReceivers.ContainsKey(permission)
				|| !this.PermissionsoToSendersToReceivers[permission].ContainsKey(oldSender))
			{
				return false;
			}

			if (this.PermissionsoToSendersToReceivers[permission].Remove(oldSender))
			{
				this.AddPermission(newSender, permission, newReceiver);
			}

			return true;
		}

		public bool RemovePermission(HashSet<Address> sender, Permission permission)
		{
			if (permission == null || !this.PermissionsoToSendersToReceivers.ContainsKey(permission)
				|| !this.PermissionsoToSendersToReceivers[permission].ContainsKey(sender))
			{
				return false;
			}

			return this.PermissionsoToSendersToReceivers[permission].Remove(sender);
		}
	}
}