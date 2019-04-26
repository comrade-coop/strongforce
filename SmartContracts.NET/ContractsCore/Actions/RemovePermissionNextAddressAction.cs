using ContractsCore.Permissions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContractsCore.Actions
{
	public class RemovePermissionNextAddressAction : RemovePermissionAction
	{
		public RemovePermissionNextAddressAction(string hash, Address target, Permission permission, object permittedAddress, object nextAddress)
			: base(hash, target, permission, permittedAddress, nextAddress)
		{
			if (permittedAddress == null)
			{
				throw new ArgumentNullException(nameof(AddPermissionExecutorAction));
			}
		}
	}
}