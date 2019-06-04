using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using StrongForce.Core;
using Tendermint;
using Tendermint.Tests.Mocks;
using Xunit;

namespace Tendermint.Tests
{
	public partial class StrongForceServerTests
	{
		[Fact]
		public void Serializes_And_Deserializes_Actions()
		{
			var server = new StrongForceServer(NullLogger<StrongForceServer>.Instance);
			var action = new DummyAction(string.Empty, new RandomAddressFactory().Create());

			var serializedAction = server.SerializeAction(action);

			var deserializedAction = server.DeserializeAction(serializedAction);

			Assert.Equal(action.GetType(), deserializedAction.GetType());
			Assert.Equal(action.Target, deserializedAction.Target);
		}

		[Fact]
		public void Does_Not_Deserialize_NonActions()
		{
			var server = new StrongForceServer(NullLogger<StrongForceServer>.Instance);

			Assert.Throws<JsonSerializationException>(() => server.DeserializeAction(
				System.Text.Encoding.UTF8.GetBytes("{\"$type\":\"Tendermint.Tests.Mocks.DummyContract, Tendermint.Tests\",\"Address\":\"Ey1dFIKIWE7PVUXemnOxk2pqKmM=\"}")));
			Assert.Throws<JsonSerializationException>(() => server.DeserializeAction(
				System.Text.Encoding.UTF8.GetBytes("{\"$type\":\"Tendermint.Tests.Mocks.DummyAction, Tendermint.Tests\",\"Target\":\"Ey1dFIKIWE7PVUXemnOxk2pqKmM=\",\"ObjectProperty\":{\"$type\":\"Tendermint.Tests.Mocks.DummyContract, Tendermint.Tests\",\"Address\":\"Ey1dFIKIWE7PVUXemnOxk2pqKmM=\"}}")));
		}
	}
}