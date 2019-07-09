using System;
using System.Runtime.Serialization;

namespace ContractsCore
{
	[DataContract]
	public class Address : IComparable<Address>
	{
		public Address(byte[] value)
		{
			this.Value = value;
		}

		[DataMember]
		public byte[] Value { get; private set; }

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
				if (this.Value == null)
				{
					return 0;
				}
				int hash = 17;
				foreach (byte element in this.Value)
				{
					hash = hash * 31 + element.GetHashCode();
				}
				return hash;
			}
		}

		public int CompareTo(Address other)
		{
			if (this.Value.Length != other.Value.Length)
			{
				throw new ArgumentException("Addresses must be of equal length.");
			}

			int comparisonResult = 0;

			for (var i = 0; i < this.Value.Length && comparisonResult == 0; i++)
			{
				int byteComparison = this.Value[i].CompareTo(other.Value[i]);
				comparisonResult = Math.Sign(byteComparison);
			}

			return comparisonResult;
		}

		public string ToBase64String()
		{
			return Convert.ToBase64String(this.Value, 0, this.Value.Length);
		}

		public static Address FromBase64String(string base64String)
		{
			return new Address(Convert.FromBase64String(base64String));
		}
	}
}