using System;
using System.Collections.Generic;
using System.Linq;
using StrongForce.Core;
using StrongForce.Core.Extensions;

namespace StrongForce.Core.Permissions
{
	public class AccessControlList : IStatefulObject
	{
		public const Address AnyAddress = Permission.AnyAddress;

		public const string AnyAction = Permission.AnyAction;

		public AccessControlList()
		{
		}

		public AccessControlList(IEnumerable<Permission> initialPermissions)
		{
			this.Permissions = new SortedSet<Permission>(initialPermissions);
		}

		public SortedSet<Permission> Permissions { get; set; } = new SortedSet<Permission>();

		public IDictionary<string, object> GetState()
		{
			var state = new Dictionary<string, object>();

			state.Set("Permissions", this.Permissions.Select(p => new Dictionary<string, object>()
			{
				{ "Type", p.Type },
				{ "Sender", p.Sender?.ToString() },
				{ "Target", p.Target?.ToString() },
			}).ToList());

			return state;
		}

		public void SetState(IDictionary<string, object> state)
		{
			this.Permissions = new SortedSet<Permission>(
				state.GetList<IDictionary<string, object>>("Permissions")
				.Select(s =>
				{
					return new Permission(
						s.Get<string>("Type"),
						s.Get<Address>("Sender"),
						s.Get<Address>("Target"));
				}));
		}

		public bool HasPermission(Permission pattern)
		{
			return this.HasPermission(pattern.Sender, pattern.Type, pattern.Target);
		}

		public bool HasPermission(Address sender, string type, Address target)
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

		public bool AddPermission(Address sender, string type, Address target)
		{
			return this.AddPermission(new Permission(type, sender, target));
		}

		public bool RemovePermission(Permission newPermission)
		{
			return this.Permissions.Remove(newPermission);
		}

		public bool RemovePermission(Address sender, string type, Address target)
		{
			return this.RemovePermission(new Permission(type, sender, target));
		}
	}
}