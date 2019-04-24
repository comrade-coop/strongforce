using System;
using System.Collections.Generic;
using System.Text;

namespace ContractsCore.Permissions
{
	public class AnyWildCard : IWildCard
	{
		public bool IsMember(Address member)
		{
			return true;
		}

		public List<Address> GetMembers()
		{
			return new List<Address>();
		}

		public bool Equal(IWildCard member)
		{
			if (!typeof(AnyWildCard).IsAssignableFrom(member.GetType()))
			{
				return false;
			}

			return true;
		}
	}
}