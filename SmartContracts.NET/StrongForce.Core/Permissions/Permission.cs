using System;
using StrongForce.Core;

namespace StrongForce.Core.Permissions
{
	public class Permission : IComparable<Permission>
	{
		public const Address AnyAddress = null;

		public const string AnyAction = null;

		public Permission(string type, Address sender, Address target)
		{
			this.Type = type;
			this.Sender = sender;
			this.Target = target;
		}

		public string Type { get; }

		public Address Sender { get; }

		public Address Target { get; }

		public override string ToString()
		{
			return $"{this.Sender?.ToString() ?? "*"} -{this.Type}-> {this.Target?.ToString() ?? "*"}";
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
			// return ValueTuple.Create(this.Type, this.Sender, this.Target).GetHashCode();
			return this.ToString().GetHashCode();
		}

		public int CompareTo(Permission other)
		{
			// return ValueTuple.Create(this.Type, this.Sender, this.Target).CompareTo(ValueTuple.Create(other.Type, other.Sender, other.Target));
			return this.ToString().CompareTo(other.ToString());
		}

		protected bool Equals(Permission other)
		{
			// return this.Type == other.Type && this.Sender == other.Sender && this.Target == other.Target;
			return this.ToString() == other.ToString();
		}
	}
}