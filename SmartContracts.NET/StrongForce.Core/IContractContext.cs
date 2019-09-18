using System;
using System.Collections.Generic;

namespace StrongForce.Core
{
	public interface IContractContext
	{
		Address Address { get; }

		void SendMessage(Address[] addresses, string type, IDictionary<string, object> payload);

		void ForwardMessage(ulong id);

		Address CreateContract<T>(IDictionary<string, object> payload)
			where T : BaseContract, new();
	}
}