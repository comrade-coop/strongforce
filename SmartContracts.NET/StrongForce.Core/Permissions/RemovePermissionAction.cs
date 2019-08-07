using System;
using System.Collections.Generic;

namespace StrongForce.Core.Permissions
{
	public class RemovePermissionAction : Action
	{
		public RemovePermissionAction(
			Address target,
			Permission permission)
			: base(target)
		{
			this.Permission = permission;
		}

		public RemovePermissionAction(
			Address target,
			Type permission,
			Address permissionSender,
			Address permissionTarget)
			: this(target, new Permission(permission, permissionSender, permissionTarget))
		{
		}

		public RemovePermissionAction(
			Address target,
			Type permission,
			Address permissionSender)
			: this(target, permission, permissionSender, target)
		{
		}

		public Permission Permission { get; }
	}
}