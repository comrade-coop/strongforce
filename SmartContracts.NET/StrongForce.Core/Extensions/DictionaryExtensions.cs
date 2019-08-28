using System;
using System.Collections.Generic;
using System.Linq;

namespace StrongForce.Core.Extensions
{
	public static class DictionaryExtensions
	{
		public static object Get(this IDictionary<string, object> dictionary, string key)
		{
			return dictionary.TryGetValue(key, out object value) ? value : null;
		}

		public static IDictionary<string, object> Set(this IDictionary<string, object> dictionary, string key, object value)
		{
			dictionary.Add(key, ConvertToState(value));
			return dictionary; // chainable
		}

		public static T Get<T>(this IDictionary<string, object> dictionary, string key)
		{
			return (T)ConvertToType(dictionary.Get(key), typeof(T));
		}

		public static IDictionary<string, object> GetDictionary(this IDictionary<string, object> dictionary, string key)
		{
			return (IDictionary<string, object>)dictionary.Get(key);
		}

		public static IList<object> GetList(this IDictionary<string, object> dictionary, string key)
		{
			return (IList<object>)dictionary.Get(key);
		}

		public static IEnumerable<T> GetList<T>(this IDictionary<string, object> dictionary, string key)
		{
			return dictionary.GetList(key).Select(x => (T)ConvertToType(x, typeof(T)));
		}

		public static IDictionary<string, object> MergeStateWith(this IDictionary<string, object> dictionary, IDictionary<string, object> other)
		{
			var newDictionary = new Dictionary<string, object>();

			foreach (var kv in dictionary)
			{
				if (other.ContainsKey(kv.Key))
				{
					var overrideValue = other[kv.Key];
					if (overrideValue == null || kv.Value == null || overrideValue.GetType() == kv.Value.GetType())
					{
						if (
							overrideValue is IDictionary<string, object> overrideState &&
							kv.Value is IDictionary<string, object> currentState)
						{
							newDictionary[kv.Key] = currentState.MergeStateWith(overrideState);
						}
						else if (
							overrideValue is IList<object> overrideList &&
							kv.Value is IList<object> currentList)
						{
							newDictionary[kv.Key] = currentList.Concat(overrideList).ToList();
						}
						else
						{
							newDictionary[kv.Key] = overrideValue;
						}
					}
					else
					{
						newDictionary[kv.Key] = kv.Value;
					}
				}
				else
				{
					newDictionary[kv.Key] = kv.Value;
				}
			}

			return newDictionary;
		}

		public static object ConvertToType(object value, Type type)
		{
			if (type == typeof(IDictionary<string, object>) || type == typeof(IList<object>) || type == typeof(object))
			{
				return value; // Caller should cast either way
			}
			else if (type == typeof(Address))
			{
				return value == null ? null : Address.Parse(value.ToString());
			}
			else if (Type.GetTypeCode(type) != TypeCode.Object)
			{
				return Convert.ChangeType(value, type);
			}
			else
			{
				throw new InvalidCastException("Cannot convert object to " + type);
			}
		}

		public static object ConvertToState(object value)
		{
			var type = value?.GetType();
			if (type == null || Type.GetTypeCode(type) != TypeCode.Object)
			{
				return value;
			}
			else if (value is Address address)
			{
				return address.ToString();
			}
			else if (value is IDictionary<string, object> dictionary)
			{
				return dictionary.ToDictionary(kv => kv.Key, kv => ConvertToState(kv.Value));
			}
			else if (value is IEnumerable<object> list)
			{
				return list.Select(e => ConvertToState(e)).ToList();
			}
			else
			{
				// NOTE: IDictionary<string, ValueType> and IEnumerable<ValueType> do not work due to variance rules in c#
				throw new InvalidCastException("Cannot convert object of type " + type + " to state");
			}
		}
	}
}