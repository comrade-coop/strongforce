using ContractsCore.Permissions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContractsCore.Actions
{
	public class AddPermissionNextAddressAction : AddPermissionAction
	{
		public AddPermissionNextAddressAction(string hash, Address target, Permission permission, object permittedAddress, object nextAddress = null)
			: base(hash, target, permission, permittedAddress, nextAddress)
		{
			if (permittedAddress == null)
			{
				throw new ArgumentNullException(nameof(AddPermissionExecutorAction));
			}
		}
	}
}