using System;
using System.Collections.Generic;
using System.Linq;

namespace StrongForce.Core.Permissions
{
	public class AccessControlList
	{
		public IDictionary<Permission, IDictionary<Address, HashSet<Address>>> PermissionsoToReceiverToSenders { get; set; }

		public AccessControlList(IDictionary<Permission, IDictionary<Address, HashSet<Address>>> initialWildCards)
			=> this.PermissionsoToReceiverToSenders = initialWildCards;

		public AccessControlList()
			: this(new SortedDictionary<Permission, IDictionary<Address, HashSet<Address>>>())
		{
		}

		public IEnumerable<Address> GetPermittedAddresses(Permission permission, Address target)
		{
			var sendersToReceivers = this.PermissionsoToReceiverToSenders[permission];

			return this.PermissionsoToReceiverToSenders[permission][target];
		}

		public bool HasPermission(Address sender, Permission permission, Address receiver)
		{
			if (sender == null || permission == null || receiver == null || !this.PermissionsoToReceiverToSenders.ContainsKey(permission))
			{
				return false;
			}

			var permissionCheck = this.PermissionsoToReceiverToSenders[permission]
				.Where(pair =>
					(pair.Key.Equals(receiver) && pair.Value.Contains(sender)) ||
					(pair.Key.Equals(receiver) && pair.Value.Contains(Address.Null())) ||
					(pair.Key.Equals(Address.Null()) && pair.Value.Contains(sender)) ||
					(pair.Key.Equals(Address.Null()) && pair.Value.Contains(Address.Null())))
				.Select(pair => pair.Value);

			int count = permissionCheck.Count();
			return count > 0;
		}

		public bool AddPermission(HashSet<Address> sender, Permission permission, Address receiver)
		{
			if (permission == null)
			{
				return false;
			}

			if (!this.PermissionsoToReceiverToSenders.ContainsKey(permission))
			{
				this.PermissionsoToReceiverToSenders[permission] =
					new SortedDictionary<Address, HashSet<Address>>()
					{
						{ receiver, sender },
					};
				return true;
			}
			else
			{
				if (this.PermissionsoToReceiverToSenders[permission].ContainsKey(receiver))
				{
					HashSet<Address> old = new HashSet<Address>(this.PermissionsoToReceiverToSenders[permission][receiver]);
					this.PermissionsoToReceiverToSenders[permission][receiver].UnionWith(sender);
					return !old.SetEquals(this.PermissionsoToReceiverToSenders[permission][receiver]);
				}
				else
				{
					this.PermissionsoToReceiverToSenders[permission].Add(receiver, sender);
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

			return this.AddPermission(senderCard, permission, receiver);
		}

		public bool RemovePermittedAddress(HashSet<Address> sender, Permission permission, Address receiver)
		{
			if (receiver == null || permission == null || !this.PermissionsoToReceiverToSenders.ContainsKey(permission)
				|| !this.PermissionsoToReceiverToSenders[permission].ContainsKey(receiver))
			{
				return false;
			}

			this.PermissionsoToReceiverToSenders[permission][receiver].ExceptWith(sender);
			return true;
		}

		public bool RemovePermission(Permission permission, Address receiver)
		{
			if (receiver == null || permission == null || !this.PermissionsoToReceiverToSenders.ContainsKey(permission)
				|| !this.PermissionsoToReceiverToSenders[permission].ContainsKey(receiver))
			{
				return false;
			}

			return this.PermissionsoToReceiverToSenders[permission].Remove(receiver);
		}
	}
}