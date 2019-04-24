using System;
using System.Collections.Generic;
using System.Text;

namespace ContractsCore.Permissions
{
	public interface IWildCard
	{
		bool IsMember(Address member);

		bool Equal(IWildCard member);

		List<Address> GetMembers();
	}
}