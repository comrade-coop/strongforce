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

		public static IDictionary<string, object> ToState(this IStateObject stateObject)
		{
			return new Dictionary<string, object>()
			{
				{ "Type", stateObject.GetType().AssemblyQualifiedName },
				{ "State", stateObject.GetState() },
			};
		}

		public static IStateObject ToStateObject(this IDictionary<string, object> dictionary)
		{
			var type = Type.GetType(dictionary.GetString("Type"));
			var state = dictionary.GetDictionary("State");

			if (!type.GetInterfaces().Contains(typeof(IStateObject)))
			{
				throw new ArgumentOutOfRangeException(nameof(type));
			}

			var stateObject = Activator.CreateInstance(type) as IStateObject;

			stateObject.SetState(state);

			return stateObject;
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
	}
}