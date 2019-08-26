using System;
using System.Collections.Generic;

namespace StrongForce.Core
{
	public struct ContractHandlers
	{
		public Action<Address[], string, IDictionary<string, object>> SendMessage;

		public Action<ulong> ForwardMessage;

		public Func<Type, IDictionary<string, object>, Address> CreateContract;
	}
}