using System;
using System.Collections.Generic;

namespace StrongForce.Core
{
	public interface IStatefulObject
	{
		IDictionary<string, object> GetState();
	}
}