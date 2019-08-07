using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace StrongForce.Core.Serialization
{
	public class AddressJsonConverter : JsonConverter<Address>
	{
		// via https://stackoverflow.com/a/26354677
		public override void WriteJson(JsonWriter writer, Address value, JsonSerializer serializer)
		{
			var serialized = value.ToBase64String();
			serialized = serialized.TrimEnd('=');
			serialized = serialized.Replace('+', '-').Replace('/', '_');
			writer.WriteValue(serialized);
		}

		public override Address ReadJson(JsonReader reader, Type objectType, Address existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			var value = (string)reader.Value;
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
			else
			{
				return null;
			}
		}
	}
}