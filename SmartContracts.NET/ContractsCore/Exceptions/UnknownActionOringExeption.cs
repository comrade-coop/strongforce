using System;
using System.Collections.Generic;
using System.Text;

namespace ContractsCore.Exceptions
{
	class UnknownActionOringExeption : Exception
	{
		public UnknownActionOringExeption(Actions.Action action)
			: base($"Action \"{action.GetType()}-{action.GetHashCode()}\" has origin set by unknown way \"origin - {action.Origin}\" ")
		{
		}
	}
}