using System;
using System.Collections.Generic;
using System.Linq;
using StrongForce.Core.Extensions;

namespace StrongForce.Core
{
	public class Message
	{
		public Message(Address target, Address origin, Address sender, string type, IDictionary<string, object> payload)
		{
			this.Target = target;
			this.Origin = origin;
			this.Sender = sender;
			this.Type = type;
			this.Payload = payload;
		}

		public Address Target { get; }

		public Address Origin { get; }

		public Address Sender { get; }

		public string Type { get; }

		public IDictionary<string, object> Payload { get; }

		public virtual Address FinalTarget { get => this.Target; }

		public static Message Create(IDictionary<string, object> state)
		{
			if (!state.ContainsKey("ForwardId"))
			{
				return new Message(
					state.Get<Address>("Target"),
					state.Get<Address>("Origin"),
					state.Get<Address>("Sender"),
					state.Get<string>("Type"),
					state.GetDictionary("Payload"));
			}
			else
			{
				return new ForwardMessage(
					state.Get<Address>("Target"),
					state.Get<Address>("Origin"),
					state.Get<Address>("Sender"),
					state.Get<string>("Type"),
					state.GetDictionary("Payload"),
					state.GetList<Address>("NextTargets").ToArray(),
					ulong.Parse(state.Get<string>("ForwardId")));
			}
		}

		public virtual IDictionary<string, object> GetState()
		{
			var state = new Dictionary<string, object>();

			state.Set("Target", this.Target);
			state.Set("Origin", this.Origin);
			state.Set("Sender", this.Sender);
			state.Set("Type", this.Type);
			state.Set("Payload", this.Payload);

			return state;
		}
	}
}