using System;
using System.Collections.Generic;
using System.Text;
using ContractsCore.Permissions;

namespace ContractsCore.Actions
{
	public class AddPermissionExecutorAction : AddPermissionAction
	{
		public AddPermissionExecutorAction(
			Address target,
			Permission permission,
			object permittedAddress,
			object nextAddress)
			: base(
				target,
				permission,
				permittedAddress,
				nextAddress)
		{
			if (nextAddress == null)
			{
				throw new ArgumentNullException(nameof(AddPermissionExecutorAction));
			}
		}
	}
}