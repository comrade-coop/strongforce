using System;
using System.Linq;
using System.Collections.Generic;
using ContractsCore.Permissions.WildCard;

namespace ContractsCore.Permissions
{
	public class AccessControlList
	{
		private readonly IDictionary<Permission, IDictionary<WildCardSet, WildCardSet>> permissionsToWildCards;

		public AccessControlList(IDictionary<Permission, IDictionary<WildCardSet, WildCardSet>> initialWildCards)
		{
			this.permissionsToWildCards = initialWildCards;
		}

		public AccessControlList()
			: this(new SortedDictionary<Permission, IDictionary<WildCardSet, WildCardSet>>())
		{
		}

		public IEnumerable<Address> GetPermittedAddresses(Permission permission, Address target)
		{
			IDictionary<WildCardSet, WildCardSet> wildCardsSets = this.permissionsToWildCards[permission];
			List<Address> members = new List<Address>();

			return wildCardsSets
				.Where(cardSet => cardSet.Value.IsMember(target))
				.Aggregate(members, (current, cardSet) =>
					current.Union(cardSet.Key.GetMembers()).ToList());
		}

		public bool HasPermission(object executor, Permission permission, object nextAddress)
		{
			if (!this.permissionsToWildCards.ContainsKey(permission))
			{
				return false;
			}

			if (executor == null || nextAddress == null)
			{
				throw new ArgumentNullException();
			}

			WildCardSet exec = WildCardSet.FromObject(executor);
			WildCardSet next = WildCardSet.FromObject(nextAddress);

			IEnumerable<KeyValuePair<WildCardSet, WildCardSet>> permitedToNextAddressed = this
				.permissionsToWildCards[permission]
				.Where(x => (executor is Address ? x.Key.IsMember(executor) : x.Key.IsMember(exec)) &&
				            (nextAddress is Address ? x.Value.IsMember(nextAddress) : x.Value.IsMember(next)));

			// TODO needs more acl test
			var toNextAddressed = permitedToNextAddressed.ToList();
			return toNextAddressed.Any() &&
			       toNextAddressed.Any(pair =>
				       pair.Key.IsMember(executor) &&
				       pair.Value.IsMember(nextAddress));
		}

		public bool AddPermission(object executor, Permission permission, object nextAddress)
		{
			if ((executor == null && nextAddress == null) || permission == null)
			{
				return false;
			}

			WildCardSet exec = WildCardSet.FromObject(executor);
			WildCardSet next = WildCardSet.FromObject(nextAddress);

			if (!this.permissionsToWildCards.ContainsKey(permission))
			{
				this.permissionsToWildCards[permission] = new Dictionary<WildCardSet, WildCardSet>() {{exec, next}};
				return true;
			}

			WildCardSet executorsCheck = this.GetPermissionExecutor(permission, nextAddress);
			WildCardSet nextAddressesCheck = this.GetPermissionNextAddress(executor, permission);
			if (executorsCheck != null && nextAddressesCheck != null)
			{
				return false;
			}

			this.permissionsToWildCards[permission].Add(exec, next);

			return true;
		}

		public bool AddPermissionExecutor(object executor, Permission permission, object nextAddress)
		{
			WildCardSet executors = this.GetPermissionExecutor(permission, nextAddress);
			return executors != null && executors.AddWildCard(executor);
		}

		public bool AddPermissionNextAddress(object executor, Permission permission, object nextAddress)
		{
			WildCardSet nextAddresses = this.GetPermissionNextAddress(executor, permission);
			return nextAddresses != null && nextAddresses.AddWildCard(nextAddress);
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
			return executors != null && executors.RemoveWildCard(executor);
		}

		public bool RemovePermissionNextAddress(object executor, Permission permission, object nextAddress)
		{
			WildCardSet nextAddresses = this.GetPermissionNextAddress(executor, permission);
			return nextAddresses != null && nextAddresses.RemoveWildCard(nextAddress);
		}

		private WildCardSet GetPermissionExecutor(Permission permission, object nextAddress)
		{
			if (permission == null)
			{
				throw new NullReferenceException();
			}

			WildCardSet next = WildCardSet.FromObject(nextAddress);
			WildCardSet result = this.permissionsToWildCards[permission]
				.FirstOrDefault(x => x.Value.CompareTo(next) == 0)
				.Key;
			return result;
		}

		private WildCardSet GetPermissionNextAddress(object executors, Permission permission)
		{
			if (permission == null)
			{
				throw new NullReferenceException();
			}

			WildCardSet exec = WildCardSet.FromObject(executors);
			WildCardSet result = this.permissionsToWildCards[permission]
				.FirstOrDefault(x => x.Key.CompareTo(exec) == 0)
				.Value;
			return result;
		}
	}
}