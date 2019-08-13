using System;
using System.Collections.Generic;
using System.Linq;
using StrongForce.Core;
using StrongForce.Core.Permissions;

namespace StrongForce.Core.Tests.Mocks
{
	public class FavoriteNumberContract : Contract
	{
		public FavoriteNumberContract()
		{
		}

		public FavoriteNumberContract(Address initialAdmin)
			: base(initialAdmin)
		{
			this.Acl.AddPermission(
				initialAdmin,
				SetFavoriteNumberAction.Type,
				this.Address);
		}

		public int Number { get; set; } = 0;

		public Address LastOrigin { get; set; } = null;

		public Address LastSender { get; set; } = null;

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