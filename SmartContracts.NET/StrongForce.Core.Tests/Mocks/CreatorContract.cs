using System;
using System.Collections.Generic;
using System.Linq;
using StrongForce.Core;
using StrongForce.Core.Extensions;
using StrongForce.Core.Permissions;

namespace StrongForce.Core.Tests.Mocks
{
	public class CreatorContract : Contract
	{
		public Address LastCreatedAddress { get; set; } = null;

		public override IDictionary<string, object> GetState()
		{
			var state = base.GetState();

			state.Set("LastCreatedAddress", this.LastCreatedAddress?.ToString());

			return state;
		}

		protected override void SetState(IDictionary<string, object> state)
		{
			this.LastCreatedAddress = state.Get<Address>("LastCreatedAddress");
			base.SetState(state);
		}

		protected override void Initialize(IDictionary<string, object> payload)
		{
			if (payload.ContainsKey("User"))
			{
				this.Acl.AddPermission(
					payload.Get<Address>("User"),
					CreateContractAction.Type,
					this.Address);
			}

			base.Initialize(payload);
		}

		protected override void HandleMessage(Message message)
		{
			switch (message.Type)
			{
				case CreateContractAction.Type:
					this.HandleCreateContractAction(message.Payload);
					break;

				default:
					base.HandleMessage(message);
					return;
			}
		}

		private void HandleCreateContractAction(IDictionary<string, object> payload)
		{
			var type = Type.GetType(payload.Get<string>(CreateContractAction.ContractType));
			this.LastCreatedAddress = this.CreateContract(type, new Dictionary<string, object>()
			{
				{ "Admin", this.Address.ToString() },
				{ "User", this.Address.ToString() },
			});

			if (payload.ContainsKey(CreateContractAction.Messages))
			{
				var messages = payload.GetList<IDictionary<string, object>>(CreateContractAction.Messages);
				foreach (var message in messages)
				{
					var messageType = message.Get<string>(CreateContractAction.MessageType);
					var messagePayload = message.Get<IDictionary<string, object>>(CreateContractAction.MessagePayload);
					this.SendMessage(this.LastCreatedAddress, messageType, messagePayload);
				}
			}
		}
	}
}