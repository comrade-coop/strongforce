using System;
using Xunit;

namespace StrongForce.Core.Tests
{
	/// <summary>
	/// Contains test methods for Addresses.
	/// </summary>
	public partial class AddressTests
	{
		private const int AddressLength = 20;

		[Fact]
		public void Equals_OnSameAddressPassed_ReturnsTrue()
		{
			var random = new Random();
			byte[] addressValue = new byte[AddressLength];
			random.NextBytes(addressValue);
			var address1 = new Address(addressValue);
			var address2 = new Address(addressValue);

			Assert.Equal(address1, address2);
		}

		[Fact]
		public void Equals_OnDifferentAddressPassed_ReturnsFalse()
		{
			var random = new Random();
			byte[] address1Value = new byte[AddressLength];
			byte[] address2Value = new byte[AddressLength];
			random.NextBytes(address1Value);
			random.NextBytes(address2Value);
			var address1 = new Address(address1Value);
			var address2 = new Address(address2Value);

			Assert.NotEqual(address1, address2);
		}

		[Fact]
		public void Equals_OnRandomObjectPassed_ReturnsFalse()
		{
			var random = new Random();
			var addressValue = new byte[AddressLength];
			random.NextBytes(addressValue);
			var address = new Address(addressValue);

			Assert.NotEqual(address, new object());
		}

		[Theory]
		[MemberData(nameof(DataForComparisonTest))]
		public void CompareTo_OnEqualLengthAddresses_ReturnsCorrectResult(
			Address address1,
			Address address2,
			int expectedResult)
		{
			Assert.Equal(expectedResult, address1.CompareTo(address2));
		}

		[Fact]
		public void CompareTo_OnDifferentLengthAddresses_ThrowsArgumentException()
		{
			var address1 = new Address(new byte[10]);
			var address2 = new Address(new byte[20]);

			Assert.Throws<ArgumentException>(() => address1.CompareTo(address2));
		}
	}
}