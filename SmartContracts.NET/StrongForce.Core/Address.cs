using System;

namespace StrongForce.Core
{
	public class Address : IComparable<Address>
	{
		public Address(byte[] value)
		{
			if (value == null)
			{
				throw new ArgumentNullException(nameof(value));
			}

			this.Value = value;
		}

		public byte[] Value { get; }

		public static bool operator ==(Address a, Address b)
		{
			if (a is null)
			{
				return b is null;
			}
			else if (b is null)
			{
				return false;
			}
			else
			{
				return a.CompareTo(b) == 0;
			}
		}

		public static bool operator !=(Address a, Address b)
		{
			return !(a == b);
		}

		public static Address FromBase64String(string base64String)
		{
			return new Address(Convert.FromBase64String(base64String));
		}

		public string ToBase64String()
		{
			return Convert.ToBase64String(this.Value, 0, this.Value.Length);
		}

		public override string ToString()
		{
			return $"Address(\"{this.ToBase64String()}\")";
		}

		public override bool Equals(object other)
		{
			if (other == null || other.GetType() != typeof(Address))
			{
				return false;
			}

			return this.CompareTo((Address)other) == 0;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				foreach (byte element in this.Value)
				{
					hash = (hash * 31) + element.GetHashCode();
				}

				return hash;
			}
		}

		public int CompareTo(Address other)
		{
			if (this.Value.Length != other.Value.Length)
			{
				return this.Value.Length.CompareTo(other.Value.Length);
			}

			int comparisonResult = 0;

			for (var i = 0; i < this.Value.Length && comparisonResult == 0; i++)
			{
				comparisonResult = this.Value[i].CompareTo(other.Value[i]);
			}

			return comparisonResult;
		}
	}
}