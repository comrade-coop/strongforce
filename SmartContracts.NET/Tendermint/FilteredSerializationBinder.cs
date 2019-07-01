using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Tendermint
{
	public class FilteredSerializationBinder : SerializationBinder
	{
		public FilteredSerializationBinder()
			: base()
		{
		}

		public ISet<Type> WhitelistedTypes { get; set; } = new HashSet<Type>();

		public ISet<Type> WhitelistedBaseTypes { get; set; } = new HashSet<Type>();

		public ISet<Type> BlacklistedTypes { get; set; } = new HashSet<Type>();

		public override Type BindToType(string assemblyName, string typeName)
		{
			var type = Type.GetType(string.Format("{0}, {1}", typeName, assemblyName));

			if (type == null)
			{
				return null;
			}

			if (this.BlacklistedTypes.Contains(type))
			{
				throw new FormatException("Request used a blacklisted type");
			}

			if (this.WhitelistedTypes.Contains(type) || type.IsValueType || type == typeof(string))
			{
				return type;
			}

			{
				var testType = type;
				while (testType != null)
				{
					if (this.WhitelistedBaseTypes.Contains(testType))
					{
						return type;
					}

					testType = testType.BaseType;
				}
			}

			if (type.FindInterfaces((m, _) => this.WhitelistedBaseTypes.Contains(m), null).Length != 0)
			{
				return type;
			}

			throw new FormatException("Request used an unknown type");
		}
	}
}