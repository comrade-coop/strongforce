using System;
using ContractsCore.Permissions;

namespace ContractsCore.Actions
{
	public class RemovePermissionNextAddressAction : RemovePermissionAction
	{
		public RemovePermissionNextAddressAction(
			Address target,
			Permission permission,
			object permittedAddress,
			object nextAddress)
			: base(target, permission, permittedAddress, nextAddress)
		{
			if (permittedAddress == null)
			{
				throw new ArgumentNullException(nameof(AddPermissionExecutorAction));
			}
		}
	}
}