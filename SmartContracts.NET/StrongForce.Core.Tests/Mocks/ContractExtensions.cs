using System;
using StrongForce.Core;
using StrongForce.Core.Permissions;

namespace StrongForce.Core.Tests.Mocks
{
	public static class ContractExtensions
	{
		public static bool CheckPermission(this Contract contract, Address sender, Type type, Address target)
		{
			return contract.Acl.HasPermission(sender, type, target);
		}
	}
}