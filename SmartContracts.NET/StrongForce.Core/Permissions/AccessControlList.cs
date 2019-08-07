using System;
using System.Collections.Generic;
using System.Linq;
using StrongForce.Core;

namespace StrongForce.Core.Permissions
{
	public class AccessControlList
	{
		public const Address AnyAddress = null;

		public const Type AnyAction = null;

		public AccessControlList(ISet<Permission> permissions)
		{
			this.Permissions = permissions;
		}

		public AccessControlList()
			: this(new SortedSet<Permission>())
		{
		}

		public ISet<Permission> Permissions { get; set; }

		public bool HasPermission(Permission pattern)
		{
			return this.HasPermission(pattern.Sender, pattern.Type, pattern.Target);
		}

		public bool HasPermission(Address sender, Type type, Address target)
		{
			return this.Permissions.Contains(new Permission(type, sender, target)) ||
				this.Permissions.Contains(new Permission(type, AnyAddress, target)) ||
				this.Permissions.Contains(new Permission(type, sender, AnyAddress)) ||
				this.Permissions.Contains(new Permission(type, AnyAddress, AnyAddress)) ||
				this.Permissions.Contains(new Permission(AnyAction, sender, target)) ||
				this.Permissions.Contains(new Permission(AnyAction, AnyAddress, target)) ||
				this.Permissions.Contains(new Permission(AnyAction, sender, AnyAddress)) ||
				this.Permissions.Contains(new Permission(AnyAction, AnyAddress, AnyAddress));
		}

		public bool AddPermission(Permission permission)
		{
			return this.Permissions.Add(permission);
		}

		public bool AddPermission(Address sender, Type type, Address target)
		{
			return this.AddPermission(new Permission(type, sender, target));
		}

		public bool RemovePermission(Permission newPermission)
		{
			return this.Permissions.Remove(newPermission);
		}

		public bool RemovePermission(Address sender, Type type, Address target)
		{
			return this.RemovePermission(new Permission(type, sender, target));
		}
	}
}