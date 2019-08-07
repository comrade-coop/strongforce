using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using StrongForce.Core;
using StrongForce.Core.Permissions;

namespace StrongForce.Core.Tests.Mocks
{
	public class FavoriteNumberContract : Contract
	{
		[JsonConstructor]
		public FavoriteNumberContract(Address address)
			: base(address)
		{
		}

		public FavoriteNumberContract(Address address, Address initialAdmin)
			: base(address, initialAdmin)
		{
			this.Acl.AddPermission(
				initialAdmin,
				typeof(SetFavoriteNumberAction),
				this.Address);
		}

		public int Number { get; set; } = 0;

		public Address LastOrigin { get; set; }

		public Address LastSender { get; set; }

		public new bool CheckPermission(ActionContext context, Action action)
		{
			return base.CheckPermission(context, action);
		}

		public bool CheckPermission(Address sender, Type type, Address target)
		{
			return this.Acl.HasPermission(sender, type, target);
		}

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