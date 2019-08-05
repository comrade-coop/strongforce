using System.Collections.Generic;
using System.Linq;
using StrongForce.Core;

namespace StrongForce.Core.Tests.Mocks
{
	public class SequentialAddressFactory : IAddressFactory
	{
		public byte AddressCount { get; private set; } = 0;

		public Address Create()
		{
			this.AddressCount++;
			return new Address(new byte[1] { this.AddressCount });
		}
	}
}