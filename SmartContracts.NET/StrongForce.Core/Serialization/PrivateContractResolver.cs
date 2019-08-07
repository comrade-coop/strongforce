using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace StrongForce.Core.Serialization
{
	public class PrivateContractResolver : DefaultContractResolver
	{
		// via https://stackoverflow.com/a/24107081
		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

			var jsonProps = props.Select(x => this.CreateProperty(x, memberSerialization));
			var jsonFields = fields.Select(x => this.CreateProperty(x, memberSerialization));

			var jsonProperties = jsonProps.Union(jsonFields).ToList();

			jsonProperties.ForEach(p =>
			{
				p.Writable = true;
				p.Readable = true;
			});

			return jsonProperties;
		}
	}
}