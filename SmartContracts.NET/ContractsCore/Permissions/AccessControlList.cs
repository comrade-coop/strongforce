using System.Collections.Generic;

namespace ContractsCore.Permissions
{
	public class AccessControlList
	{
		private readonly IDictionary<Address, HashSet<Permission>> addressesToPermissions;

		public AccessControlList(IDictionary<Address, HashSet<Permission>> initialPermissions)
		{
			this.addressesToPermissions = initialPermissions;
		}

		public AccessControlList()
			: this(new SortedDictionary<Address, HashSet<Permission>>())
		{
		}

		public bool HasPermission(Address address, Permission permission)
		{
			if (!this.addressesToPermissions.ContainsKey(address))
			{
				return false;
			}

			HashSet<Permission> permissions = this.addressesToPermissions[address];
			return permissions.Contains(permission);
		}

		public bool AddPermission(Address address, Permission permission)
		{
			if (address == null || permission == null)
			{
				return false;
			}

			if (!this.addressesToPermissions.ContainsKey(address))
			{
				this.addressesToPermissions[address] = new HashSet<Permission>();
			}

			return this.addressesToPermissions[address].Add(permission);
		}

		public bool RemovePermission(Address address, Permission permission)
		{
			if (address == null || permission == null)
			{
				return false;
			}

			return this.HasPermission(address, permission) &&
				this.addressesToPermissions[address].Remove(permission);
		}
	}
}