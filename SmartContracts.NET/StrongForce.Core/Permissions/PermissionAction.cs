using System;
using System.Collections.Generic;
using StrongForce.Core.Extensions;

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

		public static Permission GetPermission(IDictionary<string, object> payload)
		{
			return new Permission(
				payload.GetString(PermissionType),
				payload.GetAddress(PermissionSender),
				payload.GetAddress(PermissionTarget));
		}
	}
}