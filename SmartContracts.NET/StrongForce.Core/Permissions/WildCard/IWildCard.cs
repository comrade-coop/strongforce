using System;
using System.Collections.Generic;
using System.Text;

namespace StrongForce.Core.Permissions
{
	public interface IWildCard : IComparable<IWildCard>
	{
		bool IsMember(Address member);

		List<Address> GetMembers();

		int GetHashCode();

		bool Equals(object other);
	}
}