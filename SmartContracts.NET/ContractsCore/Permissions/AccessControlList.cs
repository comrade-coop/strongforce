using System;
using System.Linq;
using System.Collections.Generic;

namespace ContractsCore.Permissions
{
	public class AccessControlList
	{
		private readonly IDictionary<Permission, Dictionary<WildCardSet, WildCardSet>> PermissionsToWildCards;

		public AccessControlList(IDictionary<Permission, Dictionary<WildCardSet, WildCardSet>> initialWildCards)
		{
			this.PermissionsToWildCards = initialWildCards;
		}

		public AccessControlList()
			: this(new SortedDictionary<Permission, Dictionary<WildCardSet, WildCardSet>>())
		{
		}

		public List<Address> GetPermittedAddresses(Permission permission, Address target)
		{
			Dictionary<WildCardSet, WildCardSet> WildCardsSets = this.PermissionsToWildCards[permission];
			List<Address> members = new List<Address>();
			foreach (var cardSet in WildCardsSets)
			{
				if (cardSet.Value.IsMember(target))
				{
					members = members.Union(cardSet.Key.GetMembers()).ToList();
				}
			}

			return members;
		}

		public bool HasPermission(object address, Permission permission, object nextAddress)
		{
			if (!this.PermissionsToWildCards.ContainsKey(permission))
			{
				return false;
			}

			if (address == null || nextAddress == null)
			{
				throw new ArgumentNullException();
			}

			WildCardSet exec = WildCardSet.FromObject(address);
			WildCardSet next = WildCardSet.FromObject(nextAddress);

			WildCardSet permited = this.PermissionsToWildCards[permission]
				.FirstOrDefault(x => x.Value.Contains(next))
				.Key;
			WildCardSet nextAddressed = this.PermissionsToWildCards[permission]
				.FirstOrDefault(x => x.Key.Contains(exec))
				.Value;
			// TODO needs more acl test 
			if (permited == null || next == null)
			{
				return false;
			}

			return permited.IsMember(address) && nextAddressed.IsMember(nextAddress);
		}

		public bool AddPermission(object address, Permission permission, object nextAddress)
		{
			if ((address == null && nextAddress == null) || permission == null)
			{
				return false;
			}

			WildCardSet executors = new WildCardSet();
			executors.AddWildCard(address);
			WildCardSet nextAddresses = new WildCardSet();
			nextAddresses.AddWildCard(nextAddress);
			if (!this.PermissionsToWildCards.ContainsKey(permission))
			{
				this.PermissionsToWildCards[permission] = new Dictionary<WildCardSet, WildCardSet>() { { executors, nextAddresses } };
				return true;
			}
			else
			{
				WildCardSet executorsCheck = this.GetPermissionExecutor(permission, nextAddress);
				WildCardSet nextAddressesCheck = this.GetPermissionNextAddress(address, permission);
				if (executorsCheck != null && nextAddressesCheck != null)
				{
					return false;
				}

				this.PermissionsToWildCards[permission].Add(executors, nextAddresses);
			}

			return true;
		}

		public bool AddPermissionExecutor(object address, Permission permission, object nextAddress)
		{
			WildCardSet executors = this.GetPermissionExecutor(permission, nextAddress);
			if (executors == null)
			{
				return false;
			}

			return executors.AddWildCard(address);
		}

		public bool AddPermissionNextAddress(object address, Permission permission, object nextAddress)
		{
			WildCardSet nextAddresses = this.GetPermissionNextAddress(address, permission);
			if (nextAddresses == null)
			{
				return false;
			}

			return nextAddresses.AddWildCard(address);
		}

		public bool RemovePermission(object address, Permission permission, object nextAddress)
		{
			if ((address == null && nextAddress == null) || permission == null)
			{
				return false;
			}

			if (!this.HasPermission(address, permission, nextAddress))
			{
				return false;
			}

			WildCardSet executors = this.GetPermissionExecutor(permission, nextAddress);
			WildCardSet nextAddresses = this.GetPermissionNextAddress(address, permission);
			return executors.RemoveWildCard(address) && nextAddresses.RemoveWildCard(nextAddress);
		}

		public bool RemovePermissionExecutor(object address, Permission permission, object nextAddress)
		{
			WildCardSet executors = this.GetPermissionExecutor(permission, nextAddress);
			if (executors == null)
			{
				return false;
			}

			return executors.RemoveWildCard(address);
		}

		public bool RemovePermissionNextAddress(object address, Permission permission, object nextAddress)
		{
			WildCardSet nextAddresses = this.GetPermissionNextAddress(address, permission);
			if (nextAddresses == null)
			{
				return false;
			}

			return nextAddresses.RemoveWildCard(nextAddress);
		}

		public WildCardSet GetPermissionExecutor(Permission permission, object nextAddress)
		{
			if (permission == null)
			{
				throw new NullReferenceException();
			}
			else
			{
				WildCardSet next = WildCardSet.FromObject(nextAddress);
				var result = this.PermissionsToWildCards[permission]
					.FirstOrDefault(x => x.Value.CompareTo(next) == 0)
					.Key;
				return result;
			}
		}

		public WildCardSet GetPermissionNextAddress(object executors, Permission permission)
		{
			if (permission == null)
			{
				throw new NullReferenceException();
			}
			else
			{
				WildCardSet exec = WildCardSet.FromObject(executors);
				var result = this.PermissionsToWildCards[permission]
					.FirstOrDefault(x => x.Key.CompareTo(exec) == 0)
					.Value;
				return result;
			}
		}
	}
}