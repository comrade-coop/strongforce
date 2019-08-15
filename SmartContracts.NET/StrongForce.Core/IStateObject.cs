using System;
using System.Collections.Generic;

namespace StrongForce.Core
{
	public interface IStateObject
	{
		IDictionary<string, object> GetState();

		void SetState(IDictionary<string, object> state);
	}
}