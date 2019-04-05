using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ContractsCore.Permissions
{
	public class AssemblyWildCard : IWildCard
	{
		public bool IsMember(Address member)
		{
			Assembly[] x = AppDomain.CurrentDomain.GetAssemblies();
			string[] p = x[0].FullName.Split(',');
			int found = Array.FindIndex(x, element => element.FullName.Split(',')[0] == member.GetType().ToString());
			return found != -1;
		}

		public List<Address> GetMembers()
		{
			throw new NotImplementedException();
		}

		public bool Equal(IWildCard member)
		{
			throw new NotImplementedException();
		}
	}
}