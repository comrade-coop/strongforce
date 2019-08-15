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

			state.Add("LastOrigin", this.LastOrigin.AsString());
			state.Add("LastSender", this.LastSender.AsString());
			state.Add("Number", this.Number);

			return state;
		}

		public override void SetState(IDictionary<string, object> state)
		{
			this.LastOrigin = state.GetOrNull<string>("LastOrigin").AsAddress();
			this.LastSender = state.GetOrNull<string>("LastSender").AsAddress();
			this.Number = state.GetOrNull<int>("Number");

			base.SetState(state);
		}

		protected override void Initialize(IDictionary<string, object> payload)
		{
			base.Initialize(payload);

			var admin = payload.GetOrNull<string>("Admin").AsAddress();

			this.Acl.AddPermission(
				admin,
				SetFavoriteNumberAction.Type,
				this.Address);
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
			this.Number = payload.GetOrNull<int>(SetFavoriteNumberAction.Number);
		}
	}
}