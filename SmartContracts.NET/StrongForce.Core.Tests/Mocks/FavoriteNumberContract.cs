using System.Collections.Generic;
using System.Linq;
using StrongForce.Core;
using StrongForce.Core.Permissions;

namespace StrongForce.Core.Tests.Mocks
{
	public class FavoriteNumberContract : Contract
	{
		public FavoriteNumberContract(Address address)
			: base(address)
		{
		}

		public FavoriteNumberContract(Address address, Address initialAdmin)
			: base(address, initialAdmin)
		{
			this.Acl.AddPermission(
				initialAdmin,
				new Permission(typeof(SetFavoriteNumberAction)),
				this.Address);
		}

		public int Number { get; private set; }

		public Address LastOrigin { get; private set; }

		public Address LastSender { get; private set; }

		public new bool CheckPermission(Action action)
		{
			return base.CheckPermission(action);
		}

		public bool CheckPermission(Address sender, Permission permission, Address target)
		{
			return this.Acl.HasPermission(sender, permission, target);
		}

		protected override bool HandleAction(Action action)
		{
			this.LastOrigin = action.Origin;
			this.LastSender = action.Sender;
			switch (action)
			{
				case SetFavoriteNumberAction favoriteNumberAction:
					this.HandleSetNumberAction(favoriteNumberAction);
					return true;

				default:
					return base.HandleAction(action);
			}
		}

		private void HandleSetNumberAction(SetFavoriteNumberAction favoriteNumberAction)
		{
			this.Number = favoriteNumberAction.Number;
		}
	}
}