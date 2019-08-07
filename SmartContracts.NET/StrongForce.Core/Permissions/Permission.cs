using System;
using StrongForce.Core;

namespace StrongForce.Core.Permissions
{
	public class Permission : IComparable<Permission>
	{
		public Permission(Type type, Address sender, Address target)
		{
			this.Sender = sender;
			this.Target = target;
			this.Type = type;
		}

		public Type Type { get; }

		public Address Sender { get; }

		public Address Target { get; }

		public override string ToString()
		{
			return $"{this.Sender} - {this.Type.Name} -> {this.Target}";
		}

		public override bool Equals(object obj)
		{
			if (obj == null || obj.GetType() != typeof(Permission))
			{
				return false;
			}

			return this.Equals((Permission)obj);
		}

		public override int GetHashCode()
		{
			return ValueTuple.Create(this.Type?.Name, this.Sender, this?.Target).GetHashCode();
		}

		public int CompareTo(Permission other)
		{
			return ValueTuple.Create(this.Type?.Name, this.Sender, this.Target).CompareTo(ValueTuple.Create(other.Type?.Name, other.Sender, other.Target));
		}

		protected bool Equals(Permission other)
		{
			return this.Type == other.Type && this.Sender == other.Sender && this.Target == other.Target;
		}
	}
}