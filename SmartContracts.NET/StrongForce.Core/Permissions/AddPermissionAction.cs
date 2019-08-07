using System;
using System.Collections.Generic;

namespace StrongForce.Core.Permissions
{
	public class AddPermissionAction : Action
	{
		public AddPermissionAction(
			Address target,
			Permission permission)
			: base(target)
		{
			this.Permission = permission;
		}

		public AddPermissionAction(
			Address target,
			Type permission,
			Address permissionSender,
			Address permissionTarget)
			: this(target, new Permission(permission, permissionSender, permissionTarget))
		{
		}

		public AddPermissionAction(
			Address target,
			Type permission,
			Address permissionSender)
			: this(target, permission, permissionSender, target)
		{
		}

		public Permission Permission { get; }
	}
}