using Newtonsoft.Json;
using StrongForce.Core;
using StrongForce.Core.Serialization;
using StrongForce.Core.Tests.Mocks;
using Xunit;

namespace StrongForce.Core.Tests
{
	public partial class StrongForceSerializationTests
	{
		/*
		[Fact]
		public void Serializes_And_Deserializes_Actions()
		{
			var address = new Address(new byte[] { 10, 20, 127, 54, 51 });
			var action = new SetFavoriteNumberAction(address, 12);

			var serializedAction = StrongForceSerialization.SerializeAction(action);

			var deserializedAction = StrongForceSerialization.DeserializeAction(serializedAction);

			Assert.Equal(action.GetType(), deserializedAction.GetType());
			Assert.Equal(action.Target, deserializedAction.Target);
			Assert.Equal(action.Number, ((SetFavoriteNumberAction)deserializedAction).Number);
		}

		[Fact]
		public void Serializes_And_Deserializes_Null_Address()
		{
			var action = new SetFavoriteNumberAction(null, 12);

			var serializedAction = StrongForceSerialization.SerializeAction(action);

			var deserializedAction = StrongForceSerialization.DeserializeAction(serializedAction);

			Assert.Equal(action.GetType(), deserializedAction.GetType());
			Assert.Equal(action.Target, deserializedAction.Target);
			Assert.Equal(action.Number, ((SetFavoriteNumberAction)deserializedAction).Number);
		}

		[Fact]
		public void Does_Not_Deserialize_NonActions()
		{
			StrongForceSerialization.DeserializeAction(
				"{\"$type\":\"StrongForce.Core.Tests.Mocks.SetFavoriteNumberAction, StrongForce.Core.Tests\",\"Target\":\"Ey1dFIKIWE7PVUXemnOxk2pqKmM=\"}");
			Assert.Throws<JsonSerializationException>(() => StrongForceSerialization.DeserializeAction(
				"{\"$type\":\"StrongForce.Core.Tests.Mocks.FavoriteNumberContract, StrongForce.Core.Tests\",\"Address\":\"Ey1dFIKIWE7PVUXemnOxk2pqKmM=\"}"));
			Assert.Throws<JsonSerializationException>(() => StrongForceSerialization.DeserializeAction(
				"{\"$type\":\"StrongForce.Core.Tests.Mocks.SetFavoriteNumberAction, StrongForce.Core.Tests\",\"Target\":\"Ey1dFIKIWE7PVUXemnOxk2pqKmM=\",\"ObjectProperty\":{\"$type\":\"StrongForce.Core.Tests.Mocks.FavoriteNumberContract, StrongForce.Core.Tests\",\"Address\":\"Ey1dFIKIWE7PVUXemnOxk2pqKmM=\"}}"));
		}
		*/
	}
}