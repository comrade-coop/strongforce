// Copyright (c) Comrade Coop. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ContractsCore.Permissions
{
	public class AddressWildCard : HashSet<Address>, IWildCard
	{
		public bool IsMember(Address member)
		{
			return this.Contains(member as Address);
		}

		public List<Address> GetMembers()
		{
			return this.ToList();
		}

		public bool Equal(IWildCard member)
		{
			if (!typeof(AddressWildCard).IsAssignableFrom(member.GetType()))
			{
				return false;
			}

			foreach (var addr in member as AddressWildCard)
			{
				if (!this.IsMember(addr)) return false;
			}

			return true;
		}
	}
}