using System;
using System.Collections.Generic;
using System.Linq;
using StrongForce.Core.Extensions;

namespace StrongForce.Core
{
	public class ContractRegistryState : StatefulObject
	{
		public BaseAddressFactory AddressFactory { get; set; } = new HashedAddressFactory();

		public IDictionary<ulong, Message> ForwardedMessages { get; set; } = new Dictionary<ulong, Message>();

		public ulong MessageNonce { get; set; } = 0;

		public override IDictionary<string, object> GetState()
		{
			var state = new Dictionary<string, object>();

			state.Set("AddressFactory", new Dictionary<string, object>()
			{
				{ "Type", this.AddressFactory.GetType().AssemblyQualifiedName },
				{ "State", this.AddressFactory.GetState() },
			});

			state.Set("ForwardedMessages", this.ForwardedMessages.ToDictionary(
				kv => (string)kv.Key.ToString(),
				kv =>
				{
					var dictionary = new Dictionary<string, object>();
					dictionary.Set("Sender", kv.Value.Sender);
					dictionary.Set("Origin", kv.Value.Origin);
					dictionary.Set("Target", kv.Value.Target);
					dictionary.Set("Type", kv.Value.Type);
					dictionary.Set("Payload", kv.Value.Payload);

					if (kv.Value is ForwardMessage forwardMessage)
					{
						dictionary.Set("NextTargets", forwardMessage.NextTargets.Select(x => (object)x.ToString()).ToList());
						dictionary.Set("ForwardId", forwardMessage.ForwardId.ToString());
					}

					return (object)dictionary;
				}));

			state.Set("MessageNonce", this.MessageNonce.ToString());

			return state;
		}

		protected override void SetState(IDictionary<string, object> state)
		{
			var factory = state.Get<IDictionary<string, object>>("AddressFactory");
			var factoryType = Type.GetType(factory.Get<string>("Type"));
			var factoryState = factory.Get<IDictionary<string, object>>("State");

			this.AddressFactory = (BaseAddressFactory)StatefulObject.Create(factoryType, factoryState);

			this.ForwardedMessages = state.GetDictionary("ForwardedMessages").ToDictionary(
				kv => ulong.Parse(kv.Key),
				kv =>
				{
					if (kv.Value is IDictionary<string, object> dictionary)
					{
						if (dictionary.ContainsKey("ForwardId"))
						{
							return new ForwardMessage(
								dictionary.Get<Address>("Target"),
								dictionary.Get<Address>("Origin"),
								dictionary.Get<Address>("Sender"),
								dictionary.Get<string>("Type"),
								dictionary.GetDictionary("Payload"),
								dictionary.GetList<Address>("NextTargets").ToArray(),
								ulong.Parse(dictionary.Get<string>("ForwardId")));
						}
						else
						{
							return new Message(
								dictionary.Get<Address>("Target"),
								dictionary.Get<Address>("Origin"),
								dictionary.Get<Address>("Sender"),
								dictionary.Get<string>("Type"),
								dictionary.GetDictionary("Payload"));
						}
					}

					return null;
				});

			this.MessageNonce = ulong.Parse(state.Get<string>("MessageNonce"));
		}
	}
}