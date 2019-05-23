using System;

namespace ContractsCore.Exceptions
{
	public class UnknownContractException : ArgumentException
	{
		public UnknownContractException(Address address)
			: base($"Contract with address {address} not found")
		{
		}
	}
}