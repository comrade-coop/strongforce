using System;
using System.Collections.Generic;
using System.Text;

namespace StrongForce.Core.Permissions
{
	public class AnyWildCard : WildCard
	{
		public override bool IsMember(Address member)
		{
			return true;
		}

		public override List<Address> GetMembers()
		{
			return new List<Address>();
		}
	}
}