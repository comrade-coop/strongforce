using System;
using System.Collections.Generic;
using System.Linq;

namespace StrongForce.Core.Permissions
{
	public class AccessControlList
	{
		public AccessControlList(IDictionary<Permission, IDictionary<Address, HashSet<Address>>> initialWildCards)
			=> this.PermissionsoToReceiverToSenders = initialWildCards;

		public AccessControlList()
			: this(new SortedDictionary<Permission, IDictionary<Address, HashSet<Address>>>())
		{
		}

		public IDictionary<Permission, IDictionary<Address, HashSet<Address>>> PermissionsoToReceiverToSenders { get; set; }

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

			bool check1 = false, check2 = false;
			this.PermissionsoToReceiverToSenders[permission].TryGetValue(receiver, out HashSet<Address> perm1);
			this.PermissionsoToReceiverToSenders[permission].TryGetValue(Address.Null, out HashSet<Address> perm2);
			if (perm1 != null)
			{
				check1 = perm1.Contains(sender) || perm1.Contains(Address.Null);
			}

			if (perm2 != null)
			{
				check2 = perm2.Contains(sender) || perm2.Contains(Address.Null);
			}

			return check1 || check2;
		}

		public bool AddPermission(Address sender, Permission permission, Address receiver)
		{
			if (permission == null)
			{
				return false;
			}

			if (!this.PermissionsoToReceiverToSenders.ContainsKey(permission))
			{
				this.PermissionsoToReceiverToSenders[permission] = new SortedDictionary<Address, HashSet<Address>>();
			}

			if (!this.PermissionsoToReceiverToSenders[permission].ContainsKey(receiver))
			{
				this.PermissionsoToReceiverToSenders[permission].Add(receiver, new HashSet<Address>());
			}

			return this.PermissionsoToReceiverToSenders[permission][receiver].Add(sender);
		}

		public bool RemovePermittedAddress(Address sender, Permission permission, Address receiver)
		{
			if (receiver == null || permission == null || !this.PermissionsoToReceiverToSenders.ContainsKey(permission)
				|| !this.PermissionsoToReceiverToSenders[permission].ContainsKey(receiver))
			{
				return false;
			}

			this.PermissionsoToReceiverToSenders[permission][receiver].Remove(sender);
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