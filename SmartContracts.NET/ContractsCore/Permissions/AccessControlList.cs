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

		public bool HasPermission(object executor, Permission permission, object nextAddress)
		{
			if (!this.PermissionsToWildCards.ContainsKey(permission))
			{
				return false;
			}

			if (executor == null || nextAddress == null)
			{
				throw new ArgumentNullException();
			}

			WildCardSet exec = WildCardSet.FromObject(executor);
			WildCardSet next = WildCardSet.FromObject(nextAddress);

			var permitedToNextAddressed = this.PermissionsToWildCards[permission]
				.Where(x => (executor is Address ? x.Key.IsMember(executor) : x.Key.IsMember(exec)) &&
							(nextAddress is Address ? x.Value.IsMember(nextAddress) : x.Value.IsMember(next)));

			// TODO needs more acl test
			if (permitedToNextAddressed.Count() == 0)
			{
				return false;
			}

			foreach (var pair in permitedToNextAddressed)
			{
				if (pair.Key.IsMember(executor) && pair.Value.IsMember(nextAddress))
				{
					return true;
				}
			}

			return false;
		}

		public bool AddPermission(object executor, Permission permission, object nextAddress)
		{
			if ((executor == null && nextAddress == null) || permission == null)
			{
				return false;
			}

			WildCardSet exec = WildCardSet.FromObject(executor);
			WildCardSet next = WildCardSet.FromObject(nextAddress);

			if (!this.PermissionsToWildCards.ContainsKey(permission))
			{
				this.PermissionsToWildCards[permission] = new Dictionary<WildCardSet, WildCardSet>() { { exec, next } };
				return true;
			}
			else
			{
				WildCardSet executorsCheck = this.GetPermissionExecutor(permission, nextAddress);
				WildCardSet nextAddressesCheck = this.GetPermissionNextAddress(executor, permission);
				if (executorsCheck != null && nextAddressesCheck != null)
				{
					return false;
				}

				this.PermissionsToWildCards[permission].Add(exec, next);
			}

			return true;
		}

		public bool AddPermissionExecutor(object executor, Permission permission, object nextAddress)
		{
			WildCardSet executors = this.GetPermissionExecutor(permission, nextAddress);
			if (executors == null)
			{
				return false;
			}

			return executors.AddWildCard(executor);
		}

		public bool AddPermissionNextAddress(object executor, Permission permission, object nextAddress)
		{
			WildCardSet nextAddresses = this.GetPermissionNextAddress(executor, permission);
			if (nextAddresses == null)
			{
				return false;
			}

			return nextAddresses.AddWildCard(nextAddress);
		}

		public bool RemovePermission(object executor, Permission permission, object nextAddress)
		{
			if ((executor == null && nextAddress == null) || permission == null)
			{
				return false;
			}

			if (!this.HasPermission(executor, permission, nextAddress))
			{
				return false;
			}

			WildCardSet executors = this.GetPermissionExecutor(permission, nextAddress);
			WildCardSet nextAddresses = this.GetPermissionNextAddress(executor, permission);
			return executors.RemoveWildCard(executor) && nextAddresses.RemoveWildCard(nextAddress);
		}

		public bool RemovePermissionExecutor(object executor, Permission permission, object nextAddress)
		{
			WildCardSet executors = this.GetPermissionExecutor(permission, nextAddress);
			if (executors == null)
			{
				return false;
			}

			return executors.RemoveWildCard(executor);
		}

		public bool RemovePermissionNextAddress(object executor, Permission permission, object nextAddress)
		{
			WildCardSet nextAddresses = this.GetPermissionNextAddress(executor, permission);
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