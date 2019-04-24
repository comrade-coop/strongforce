using System;
using System.Collections.Generic;
using System.Text;

namespace ContractsCore.Exceptions
{
	class UnsupportedTypeExeption : ArgumentException
	{
		public UnsupportedTypeExeption(object origin, object type, string method)
			: base(
				$"\"{type.GetType()}\" is not supported in {origin.GetType()} {method}")
		{
		}
	}
}