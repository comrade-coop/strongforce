using System;
using ContractsCore.Permissions;

namespace ContractsCore.Actions
{
	public class RemovePermissionExecutorAction : RemovePermissionAction
	{
		public RemovePermissionExecutorAction(
			Address target,
			Permission permission,
			object permittedAddress,
			object nextAddress)
			: base(target, permission, permittedAddress, nextAddress)
		{
			if (nextAddress == null)
			{
				throw new ArgumentNullException(nameof(AddPermissionExecutorAction));
			}
		}
	}
}