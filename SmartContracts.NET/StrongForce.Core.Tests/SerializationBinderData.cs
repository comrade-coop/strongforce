using System;
using System.Collections.Generic;
using Xunit;

namespace StrongForce.Core.Tests
{
	public partial class FilteredSerializationBinderTests
	{
		public static IEnumerable<object[]> ListOfValueTypes => new List<object[]>
		{
			new object[] { typeof(int) },
			new object[] { typeof(string) }, // Not really, but safe to assume
			new object[] { typeof(float) },
			new object[] { typeof(bool) },
			new object[] { typeof(Struct) },
		};

		public static IEnumerable<object[]> ListOfTypes => new List<object[]>
		{
			new object[] { typeof(Base) },
			new object[] { typeof(Derived) },
		};

		public static IEnumerable<object[]> ListOfInterfaceTypes => new List<object[]>
		{
			new object[] { typeof(Base) },
			new object[] { typeof(Derived) },
			new object[] { typeof(OtherBase) },
		};

#pragma warning disable SA1201
		public interface IBase
#pragma warning restore SA1201
		{
		}

		public struct Struct
		{
		}

		public class Base : IBase
		{
		}

		public class Derived : Base, IBase
		{
		}

		public class OtherBase : IBase
		{
		}
	}
}