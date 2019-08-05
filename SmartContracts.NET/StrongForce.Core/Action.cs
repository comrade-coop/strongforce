using System;
using System.Runtime.Serialization;

namespace StrongForce.Core
{
	public class Action
	{
		public Action(Address target)
		{
			this.Target = target;
		}

		public Address Target { get; }
	}
}