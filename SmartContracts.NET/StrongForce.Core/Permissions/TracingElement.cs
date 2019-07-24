using System;
using System.Collections.Generic;
using System.Text;

namespace StrongForce.Core.Permissions
{
	public class TracingElement : IEquatable<TracingElement>
	{
		public TracingElement(Address address, Stack<Address> path, bool isPath = false)
		{
			this.Address = address;
			this.Path = path;
			this.IsPath = isPath;
		}

		public Address Address { get; set; }

		public Stack<Address> Path { get; set; }

		public bool IsPath { get; set; }

		public bool Equals(TracingElement other)
		{
			if (!this.Address.Equals(other.Address) || this.IsPath != other.IsPath || this.Path.Count != other.Path.Count)
			{
				return false;
			}

			foreach (var addr in this.Path)
			{
				if (!other.Path.Contains(addr))
				{
					return false;
				}
			}

			return true;
		}
	}
}