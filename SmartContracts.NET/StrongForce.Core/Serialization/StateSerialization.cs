using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace StrongForce.Core.Serialization
{
	public static class StateSerialization
	{
		public static byte[] SerializeState(IDictionary<string, object> state)
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = new Utf8JsonWriter(stream))
				{
					WriteDictionary(writer, state);
					writer.Flush();
				}

				return stream.ToArray();
			}
		}

		public static IDictionary<string, object> DeserializeState(byte[] serialized)
		{
			var reader = new Utf8JsonReader(serialized);

			reader.Read();

			return ReadDictionary(ref reader);
		}

		public static bool ValidateState(IDictionary<string, object> state)
		{
			return ValidateObject(state);
		}

		// Writing
		private static void WriteObject(Utf8JsonWriter writer, object value)
		{
			switch (value)
			{
				case bool boolValue:
					writer.WriteBooleanValue(boolValue);
					break;
				case null:
					writer.WriteNullValue();
					break;

				case string stringValue:
					writer.WriteStringValue(JsonEncodedText.Encode(stringValue));
					break;

				case int intValue:
					writer.WriteNumberValue(intValue);
					break;
				case uint uintValue:
					writer.WriteNumberValue(uintValue);
					break;
				case long longValue:
					writer.WriteNumberValue(longValue);
					break;
				case ulong ulongValue:
					writer.WriteNumberValue(ulongValue);
					break;

				case float floatValue:
					writer.WriteNumberValue(floatValue);
					break;
				case double doubleValue:
					writer.WriteNumberValue(doubleValue);
					break;
				case decimal decimalValue:
					writer.WriteNumberValue(decimalValue);
					break;

				case IEnumerable<KeyValuePair<string, object>> pairEnumerableValue:
					WriteDictionary(writer, pairEnumerableValue);
					break;

				case IEnumerable<object> enumerableValue:
					WriteArray(writer, enumerableValue);
					break;

				default:
					throw new FormatException("Could not encode " + value);
			}
		}

		private static void WriteDictionary(Utf8JsonWriter writer, IEnumerable<KeyValuePair<string, object>> dictionary)
		{
			writer.WriteStartObject();
			dictionary = dictionary.OrderBy(kv => kv.Key);

			foreach (var kv in dictionary)
			{
				writer.WritePropertyName(JsonEncodedText.Encode(kv.Key));
				WriteObject(writer, kv.Value);
			}

			writer.WriteEndObject();
		}

		private static void WriteArray(Utf8JsonWriter writer, IEnumerable<object> array)
		{
			writer.WriteStartArray();

			foreach (var v in array)
			{
				WriteObject(writer, v);
			}

			writer.WriteEndArray();
		}

		// Reading
		private static object ReadObject(ref Utf8JsonReader reader)
		{
			object value;
			switch (reader.TokenType)
			{
				case JsonTokenType.False:
					value = false;
					break;
				case JsonTokenType.True:
					value = true;
					break;
				case JsonTokenType.Null:
					value = null;
					break;
				case JsonTokenType.Number:
					if (reader.TryGetInt32(out int intValue))
					{
						value = intValue;
					}
					else if (reader.TryGetInt64(out long longValue))
					{
						value = longValue;
					}
					else
					{
						value = reader.GetDecimal();
					}

					break;
				case JsonTokenType.String:
					value = reader.GetString();
					break;

				case JsonTokenType.StartObject:
					value = ReadDictionary(ref reader);
					break;
				case JsonTokenType.StartArray:
					value = ReadArray(ref reader);
					break;

				default:
					throw new ArgumentOutOfRangeException(nameof(value));
			}

			reader.Read();

			return value;
		}

		private static IDictionary<string, object> ReadDictionary(ref Utf8JsonReader reader)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
			{
				throw new FormatException("Expected '{'");
			}

			reader.Read();

			var result = new Dictionary<string, object>();

			while (reader.TokenType == JsonTokenType.PropertyName)
			{
				var key = reader.GetString();
				reader.Read();
				var value = ReadObject(ref reader);

				result.Add(key, value);
			}

			if (reader.TokenType != JsonTokenType.EndObject)
			{
				throw new FormatException("Expected '}'");
			}

			return result;
		}

		private static IList<object> ReadArray(ref Utf8JsonReader reader)
		{
			if (reader.TokenType != JsonTokenType.StartArray)
			{
				throw new FormatException("Expected '['");
			}

			reader.Read();

			var result = new List<object>();

			while (reader.TokenType != JsonTokenType.EndArray)
			{
				var value = ReadObject(ref reader);

				result.Add(value);
			}

			return result;
		}

		// Validating
		private static bool ValidateObject(object value)
		{
			switch (value)
			{
				case null:
				case bool boolValue:
				case string stringValue:
				case int intValue:
				case uint uintValue:
				case long longValue:
				case ulong ulongValue:
				case float floatValue:
				case double doubleValue:
				case decimal decimalValue:
					return true;

				case Dictionary<string, object> pairEnumerableValue:
					foreach (var kv in pairEnumerableValue)
					{
						if (!ValidateObject(kv.Value))
						{
							return false;
						}
					}

					return true;

				case List<object> enumerableValue:
					foreach (var v in enumerableValue)
					{
						if (!ValidateObject(v))
						{
							return false;
						}
					}

					return true;

				default:
					return false;
			}
		}
	}
}