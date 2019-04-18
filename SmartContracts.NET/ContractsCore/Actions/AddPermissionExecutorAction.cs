using System;
using System.Collections.Generic;
using System.Text;
using ContractsCore.Permissions;

namespace ContractsCore.Actions
{
	public class AddPermissionExecutorAction : AddPermissionAction
	{
		public AddPermissionExecutorAction(string hash, Address target, Permission permission, object permittedAddress, object nextAddress = null)
			: base(hash, target, permission, permittedAddress, nextAddress)
		{
			if (nextAddress == null)
			{
				throw new ArgumentNullException(nameof(AddPermissionExecutorAction));
			}
		}
	}
}