using System;
using System.Collections.Generic;

namespace StrongForce.Core.Permissions
{
	public class PermissionAction
	{
		public const string PermissionType = "Permission";

		public const string PermissionSender = "PermissionSender";

		public const string PermissionTarget = "PermissionTarget";

		protected PermissionAction() // HACK: Needed in order to inherit the constants in Add/RemovePermissionAction
		{
		}

		public static Permission GetPermission(PayloadAction action)
		{
			return new Permission(
				action.Payload.GetOrNull<string>(PermissionType),
				action.Payload.GetOrNull<string>(PermissionSender).AsAddress(),
				action.Payload.GetOrNull<string>(PermissionTarget).AsAddress());
		}
	}
}