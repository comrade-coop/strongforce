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
				typeof(SetFavoriteNumberAction),
				this.Address);
		}

		public int Number { get; set; } = 0;

		public Address LastOrigin { get; set; } = null;

		public Address LastSender { get; set; } = null;

		protected override bool HandleAction(ActionContext context, Action action)
		{
			this.LastOrigin = context.Origin;
			this.LastSender = context.Sender;
			switch (action)
			{
				case SetFavoriteNumberAction favoriteNumberAction:
					this.HandleSetNumberAction(favoriteNumberAction);
					return true;

				default:
					return base.HandleAction(context, action);
			}
		}

		private void HandleSetNumberAction(SetFavoriteNumberAction favoriteNumberAction)
		{
			this.Number = favoriteNumberAction.Number;
		}
	}
}