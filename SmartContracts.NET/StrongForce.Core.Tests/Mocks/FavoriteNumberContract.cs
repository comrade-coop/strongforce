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

			state.Add("LastOrigin", this.LastOrigin?.ToBase64String());
			state.Add("LastSender", this.LastSender?.ToBase64String());
			state.Add("Number", this.Number);

			return state;
		}

		public override void SetState(IDictionary<string, object> state)
		{
			this.LastOrigin = state.GetAddress("LastOrigin");
			this.LastSender = state.GetAddress("LastSender");
			this.Number = state.Get<int>("Number");

			base.SetState(state);
		}

		protected override void Initialize(IDictionary<string, object> payload)
		{
			if (payload.ContainsKey("User"))
			{
				this.Acl.AddPermission(
					payload.GetAddress("User"),
					SetFavoriteNumberAction.Type,
					this.Address);
			}

			base.Initialize(payload);
		}

		protected override bool HandlePayloadAction(PayloadAction action)
		{
			this.LastOrigin = action.Origin;
			this.LastSender = action.Sender;
			switch (action.Type)
			{
				case SetFavoriteNumberAction.Type:
					this.HandleSetNumberAction(action.Payload);
					return true;

				default:
					return base.HandlePayloadAction(action);
			}
		}

		private void HandleSetNumberAction(IDictionary<string, object> payload)
		{
			this.Number = payload.Get<int>(SetFavoriteNumberAction.Number);
		}
	}
}