using System;
using System.Collections.Generic;
using StrongForce.Core.Extensions;

namespace StrongForce.Core
{
	/// <summary>
	/// Used to generate address values, which are to be injected in all entities in the StrongForce instance.
	/// </summary>
	public abstract class BaseAddressFactory : StatefulObject
	{
		public abstract Address CreateAddress();
	}
}