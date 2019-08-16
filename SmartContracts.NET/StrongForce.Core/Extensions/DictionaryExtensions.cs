using System;
using System.Collections.Generic;
using System.Linq;

namespace StrongForce.Core.Extensions
{
	public static class DictionaryExtensions
	{
		public static object GetOrElse(this IDictionary<string, object> dictionary, string key, object defaultValue)
		{
			return dictionary.TryGetValue(key, out object value) ? value : defaultValue;
		}

		public static T Get<T>(this IDictionary<string, object> dictionary, string key)
			where T : struct
		{
			return (T)dictionary[key];
		}

		public static T? GetNullable<T>(this IDictionary<string, object> dictionary, string key)
			where T : struct
		{
			return (T?)dictionary.GetOrElse(key, null);
		}

		public static string GetString(this IDictionary<string, object> dictionary, string key)
		{
			return (string)dictionary.GetOrElse(key, null);
		}

		public static IDictionary<string, object> GetDictionary(this IDictionary<string, object> dictionary, string key)
		{
			return (IDictionary<string, object>)dictionary.GetOrElse(key, null);
		}

		public static IList<object> GetList(this IDictionary<string, object> dictionary, string key)
		{
			return (IList<object>)dictionary.GetOrElse(key, null);
		}

		public static IEnumerable<T> GetList<T>(this IDictionary<string, object> dictionary, string key)
		{
			return dictionary.GetList(key).Cast<T>();
		}

		public static Address GetAddress(this IDictionary<string, object> dictionary, string key)
		{
			var value = dictionary.GetString(key);

			return Address.FromBase64String(value);
		}

		public static void AddAddress(this IDictionary<string, object> dictionary, string key, Address value)
		{
			var serialized = value?.ToBase64String();
			dictionary.Add(key, serialized);
		}
	}
}