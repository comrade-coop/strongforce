using System.Collections.Generic;
using StrongForce.Core;
using StrongForce.Core.Extensions;
using StrongForce.Core.Serialization;
using StrongForce.Core.Tests.Mocks;
using Xunit;

namespace StrongForce.Core.Tests
{
	public partial class StrongForceSerializationTests
	{
		[Fact]
		public void Serializes_And_Deserializes_State()
		{
			var state = new Dictionary<string, object>()
			{
				{ "testString", "string value with \n newlines and \t tabs" },
				{ "testInt", 15 },
				{ "testDecimal", 15.0m },
				{ "testBoolean", true },
				{
					"testDictionary",
					new Dictionary<string, object>()
					{
						{ "nestedString", "another string value \0 with a null byte" },
						{ "nestedInt", 42 },
						{ "nestedTrue", true },
						{ "nestedFalse", false },
					}
				},
				{
					"testList",
					new List<object>()
					{
						"A string, and an int (- - )",
						103,
						true,
					}
				},
			};

			var serialized = StateSerialization.SerializeState(state);

			var deserialized = StateSerialization.DeserializeState(serialized);
			var reserialized = StateSerialization.SerializeState(deserialized);

			Assert.Equal(serialized, reserialized);
			Assert.Equal(state, deserialized);
		}

		[Fact]
		public void Serializes_And_Deserializes_Messages()
		{
			var targets = new Address[] { new Address(new byte[] { 10, 20, 127, 54, 51 }), null };
			var type = "NotARealActionType";
			var payload = new Dictionary<string, object>()
			{
				{ "isTest", true },
			};

			var serializedMessage = StrongForceSerialization.SerializeMessage(targets, type, payload);

			var deserializedMessage = StrongForceSerialization.DeserializeMessage(serializedMessage);

			Assert.Equal(targets, deserializedMessage.Item1);
			Assert.Equal(type, deserializedMessage.Item2);
			Assert.Equal(payload, deserializedMessage.Item3);
		}

		[Fact]
		public void Serializes_And_Deserializes_Contracts()
		{
			var address = new Address(new byte[] { 10, 20, 127, 54, 51 });
			var adminAddress = new Address(new byte[] { 10, 20, 4 });
			var contract = StatefulObject.Create<FavoriteNumberContract>(new Dictionary<string, object> { { "Admin", adminAddress.ToString() }, { "Number", 15 } });
			contract.RegisterWithRegistry(new InMemoryIntegration.FakeContractContext(address));

			var serializedContract = StrongForceSerialization.SerializeStatefulObject(contract);

			var deserializedContract = (FavoriteNumberContract)StrongForceSerialization.DeserializeStatefulObject(serializedContract);
			deserializedContract.RegisterWithRegistry(new InMemoryIntegration.FakeContractContext(address));

			Assert.Equal(contract.GetState(), deserializedContract.GetState());
			Assert.Equal(contract.Address, deserializedContract.Address);
			Assert.Equal(contract.Number, deserializedContract.Number);
		}
	}
}