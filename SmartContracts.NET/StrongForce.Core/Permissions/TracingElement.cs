using System;
using System.Collections.Generic;
using System.Text;

namespace StrongForce.Core.Permissions
{
	public class TracingElement : IEquatable<TracingElement>
	{
		public TracingElement(Address address, Stack<Address> way, bool isWay = false)
		{
			this.Address = address;
			this.Way = way;
			this.IsWay = isWay;
		}

		public Address Address { get; set; }

		public Stack<Address> Way { get; set; }

		public bool IsWay { get; set; }

		public bool Equals(TracingElement other)
		{
			if (!this.Address.Equals(other.Address) || this.IsWay != other.IsWay || this.Way.Count != other.Way.Count)
			{
				return false;
			}

			foreach (var addr in this.Way)
			{
				if (!other.Way.Contains(addr))
				{
					return false;
				}
			}

			return true;
		}
	}
}