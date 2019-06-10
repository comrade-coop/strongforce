using System;
using System.Collections.Generic;
using ContractsCore.Actions;
using ContractsCore.Contracts;
using ContractsCore.Events;
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

		public Address LastSender { get; private set; }

		public Address LastOrigin { get; private set; }

		public void SetNumberInvoke(Actions.Action action)
		{
			this.OnSend(action);
		}

		protected override object GetState() => this.Number;

		protected override bool HandleReceivedAction(Actions.Action action)
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