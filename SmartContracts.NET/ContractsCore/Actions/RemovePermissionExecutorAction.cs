using ContractsCore.Permissions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContractsCore.Actions
{
	public class RemovePermissionExecutorAction : RemovePermissionAction
	{
		public RemovePermissionExecutorAction(string hash, Address target, Permission permission, object permittedAddress,
			object nextAddress)
			: base(hash, target, permission, permittedAddress, nextAddress)
		{
			if (nextAddress == null)
			{
				throw new ArgumentNullException(nameof(AddPermissionExecutorAction));
			}
		}
	}
}