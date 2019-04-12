using ContractsCore.Actions;
using ContractsCore.Contracts;
using ContractsCore.Exceptions;

namespace ContractsCore.Tests.Mocks
{
	public class FavoriteNumberContract : Contract
	{
		public FavoriteNumberContract(Address address)
			: base(address)
		{
		}

		public int Number { get; private set; }

		protected internal override object GetState() => this.Number;

		protected override bool HandleReceivedAction(Action action)
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
		}
	}
}