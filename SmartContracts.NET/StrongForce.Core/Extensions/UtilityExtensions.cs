using System;
using System.Collections.Generic;
using System.Linq;

namespace StrongForce.Core.Extensions
{
	public static class UtilityExtensions
	{
		// via https://stackoverflow.com/a/26354677
		public static Address AsAddress(this string value)
		{
			if (value != null)
			{
				value = value.Replace('_', '/').Replace('-', '+');
				switch (value.Length % 4)
				{
					case 2:
						value += "==";
						break;
					case 3:
						value += "=";
						break;
				}

				return Address.FromBase64String(value);
			}

			return null;
		}

		public static string AsString(this Address value)
		{
			if (value != null)
			{
				var serialized = value.ToBase64String();
				serialized = serialized.TrimEnd('=');
				serialized = serialized.Replace('+', '-').Replace('/', '_');

				return serialized;
			}

			return null;
		}
	}
}