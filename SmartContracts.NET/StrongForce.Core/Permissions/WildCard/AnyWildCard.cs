using System;
using System.Collections.Generic;
using System.Text;

namespace StrongForce.Core.Permissions
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

		public int CompareTo(IWildCard other)
		{
			if (this.ToString() != other.ToString())
			{
				return 1;
			}

			return 0;
		}

		public override int GetHashCode()
		{
			return this.ToString().GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return this.CompareTo(obj as AnyWildCard) == 0;
		}
	}
}