using System;
using System.Collections.Generic;
using System.Linq;
using StrongForce.Core;
using StrongForce.Core.Extensions;
using StrongForce.Core.Permissions;

namespace StrongForce.Core.Tests.Mocks
{
	public class FakeContractContext : IContractContext
	{
		public FakeContractContext(Address address)
		{
			this.Address = address;
		}

		public Address Address { get; }

		public void SendMessage(Address[] addresses, string type, IDictionary<string, object> payload)
		{
			throw new NotSupportedException();
		}

		public void ForwardMessage(ulong id)
		{
			throw new NotSupportedException();
		}

		public Address CreateContract<T>(IDictionary<string, object> payload)
			where T : BaseContract, new()
		{
			throw new NotSupportedException();
		}
	}
}