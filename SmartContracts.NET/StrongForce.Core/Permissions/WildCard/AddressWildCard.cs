// Copyright (c) Comrade Coop. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrongForce.Core.Permissions
{
	public class AddressWildCard : WildCard
	{
		public override bool IsMember(Address member)
		{
			return this.Contains(member as Address);
		}

		public override List<Address> GetMembers()
		{
			return this.ToList();
		}
	}
}