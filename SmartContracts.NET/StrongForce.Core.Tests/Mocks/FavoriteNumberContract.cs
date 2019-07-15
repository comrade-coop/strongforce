using StrongForce.Core;
using Action = StrongForce.Core.Action;

namespace StrongForce.Core.Tests.Mocks
{
	public class FavoriteNumberContract : Contract
	{
		public FavoriteNumberContract(Address address)
			: base(address)
		{
		}

		public int Number { get; private set; }

		public Address LastSender { get; private set; }

		public Address LastOrigin { get; private set; }

		public void SetNumberInvoke(Action action)
		{
			this.SendAction(action);
		}

		protected override object GetState() => this.Number;

		protected override bool HandleAction(Action action)
		{
			switch (action)
			{
				case SetFavoriteNumberAction favoriteNumberAction:
					this.HandleSetNumberAction(favoriteNumberAction);
					return true;

				default:
					return false;
			}
		}

		private void HandleSetNumberAction(SetFavoriteNumberAction favoriteNumberAction)
		{
			this.Number = favoriteNumberAction.Number;
			this.LastOrigin = favoriteNumberAction.Origin;
			this.LastSender = favoriteNumberAction.Sender;
		}
	}
}