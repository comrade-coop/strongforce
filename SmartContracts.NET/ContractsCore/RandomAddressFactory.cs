using System;

namespace ContractsCore
{
	public class RandomAddressFactory : IAddressFactory
	{
		private const int AddressLength = 20;

		private readonly Random random = new Random();

		public Address Create()
		{
			var addressValue = new byte[AddressLength];
			this.random.NextBytes(addressValue);
			return new Address(addressValue);
		}
	}
}