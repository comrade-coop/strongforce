using System.Collections.Generic;

namespace StrongForce.Core.Tests
{
	/// <summary>
	/// Contains test data for AddressTest.
	/// </summary>
	public partial class AddressTests
	{
		public static IEnumerable<object[]> DataForComparisonTest =>
			new List<object[]>
			{
				new object[]
				{
					new Address(new byte[] { 1, 1, 1 }),
					new Address(new byte[] { 1, 1, 1 }),
					0,
				},
				new object[]
				{
					new Address(new byte[] { 1, 2, 1 }),
					new Address(new byte[] { 1, 1, 1 }),
					1,
				},
				new object[]
				{
					new Address(new byte[] { 1, 1, 1 }),
					new Address(new byte[] { 1, 56, 1 }),
					-1,
				},
				new object[]
				{
					new Address(new byte[] { 23, 43, 1 }),
					new Address(new byte[] { 34, 23, 1 }),
					-1,
				},
				new object[]
				{
					new Address(new byte[] { 150, 23, 22 }),
					new Address(new byte[] { 150, 23, 21 }),
					1,
				},
			};
	}
}