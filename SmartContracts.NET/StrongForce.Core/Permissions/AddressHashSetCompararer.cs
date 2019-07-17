using System;
using System.Collections.Generic;
using System.Text;

namespace StrongForce.Core.Permissions
{
	public class AddressHashSetCompararer : IComparer<HashSet<Address>>
	{
		public int Compare(HashSet<Address> x, HashSet<Address> y)
		{
			bool equal = x.SetEquals(y);
			return equal ? 0 : 1;
		}
	}
}