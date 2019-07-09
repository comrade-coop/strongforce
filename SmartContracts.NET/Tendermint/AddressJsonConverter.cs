using System;
using System.Collections.Generic;
using ContractsCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Tendermint
{
	public class AddressJsonConverter : JsonConverter<Address>
	{
		public override void WriteJson(JsonWriter writer, Address value, JsonSerializer serializer)
		{
			writer.WriteValue(value.ToBase64String());
		}

		public override Address ReadJson(JsonReader reader, Type objectType, Address existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			return Address.FromBase64String((string)reader.Value);
		}
	}
}