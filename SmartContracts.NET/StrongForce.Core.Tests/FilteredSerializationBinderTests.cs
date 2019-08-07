using System;
using StrongForce.Core.Serialization;
using Xunit;

namespace StrongForce.Core.Tests
{
	public partial class FilteredSerializationBinderTests
	{
		[Theory]
		[MemberData(nameof(ListOfValueTypes))]
		public void Allows_ValueTypes(Type type)
		{
			var binder = new FilteredSerializationBinder();
			Assert.Equal(type, binder.BindToType(type.Assembly.FullName, type.FullName));
		}

		[Theory]
		[MemberData(nameof(ListOfValueTypes))]
		public void Disallows_Blacklisted_ValueTypes(Type type)
		{
			var binder = new FilteredSerializationBinder();
			binder.BlacklistedTypes.Add(type);
			Assert.Throws<FormatException>(() => binder.BindToType(type.Assembly.FullName, type.FullName));
		}

		[Theory]
		[MemberData(nameof(ListOfTypes))]
		public void Allows_Whitelisted_Types(Type type)
		{
			var binder = new FilteredSerializationBinder();
			binder.WhitelistedTypes.Add(type);
			Assert.Equal(type, binder.BindToType(type.Assembly.FullName, type.FullName));
		}

		[Theory]
		[MemberData(nameof(ListOfTypes))]
		public void Allows_Whitelisted_Base_Types(Type type)
		{
			var binder = new FilteredSerializationBinder();
			binder.WhitelistedBaseTypes.Add(typeof(Base));
			Assert.Equal(type, binder.BindToType(type.Assembly.FullName, type.FullName));
		}

		[Theory]
		[MemberData(nameof(ListOfInterfaceTypes))]
		public void Allows_Whitelisted_Interfaces_Types(Type type)
		{
			var binder = new FilteredSerializationBinder();
			binder.WhitelistedBaseTypes.Add(typeof(IBase));
			Assert.Equal(type, binder.BindToType(type.Assembly.FullName, type.FullName));
		}

		[Theory]
		[MemberData(nameof(ListOfTypes))]
		public void Disallows_Unknown_Types(Type type)
		{
			var binder = new FilteredSerializationBinder();
			Assert.Throws<FormatException>(() => binder.BindToType(type.Assembly.FullName, type.FullName));
		}
	}
}