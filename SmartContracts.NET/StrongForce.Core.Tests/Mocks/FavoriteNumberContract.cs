using System;
using System.Collections.Generic;
using System.Linq;
using StrongForce.Core;
using StrongForce.Core.Extensions;
using StrongForce.Core.Permissions;

namespace StrongForce.Core.Tests.Mocks
{
	public class FavoriteNumberContract : Contract
	{
		public FavoriteNumberContract()
		{
		}

		public int Number { get; set; } = 0;

		public Address LastOrigin { get; set; } = null;

		public Address LastSender { get; set; } = null;

		public override IDictionary<string, object> GetState()
		{
			var state = base.GetState();

			state.Set("LastOrigin", this.LastOrigin?.ToString());
			state.Set("LastSender", this.LastSender?.ToString());
			state.Set("Number", this.Number);

			return state;
		}

		protected override void SetState(IDictionary<string, object> state)
		{
			this.LastOrigin = state.Get<Address>("LastOrigin");
			this.LastSender = state.Get<Address>("LastSender");
			this.Number = state.Get<int>("Number");

			base.SetState(state);
		}

		protected override void Initialize(IDictionary<string, object> payload)
		{
			if (payload.ContainsKey("User"))
			{
				this.Acl.AddPermission(
					payload.Get<Address>("User"),
					SetFavoriteNumberAction.Type,
					this.Address);
			}

			base.Initialize(payload);
		}

		protected override void HandleMessage(Message message)
		{
			this.LastOrigin = message.Origin;
			this.LastSender = message.Sender;
			switch (message.Type)
			{
				case SetFavoriteNumberAction.Type:
					this.HandleSetNumberAction(message.Payload);
					break;

				default:
					base.HandleMessage(message);
					break;
			}
		}

		private void HandleSetNumberAction(IDictionary<string, object> payload)
		{
			this.Number = payload.Get<int>(SetFavoriteNumberAction.Number);
		}
	}
}