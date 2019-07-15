// Copyright (c) Comrade Coop. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrongForce.Core.Permissions
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

		public int CompareTo(IWildCard other)
		{
			if (this.ToString() != other.ToString())
			{
				return this.GetHashCode().CompareTo(other.GetHashCode());
			}

			bool equal = (this as HashSet<Address>).SetEquals(other as HashSet<Address>);
			return equal ? 0 : 1;
		}

		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return this.CompareTo(obj as AddressWildCard) == 0;
		}
	}
}