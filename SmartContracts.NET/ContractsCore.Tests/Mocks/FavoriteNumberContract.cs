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
		public FavoriteNumberContract(Address address, ContractRegistry registry = null)
			: base(address, registry)
		{
		}

		public int Number { get; private set; }

		public Address LastSender { get; private set; }

		public Address LastOrigin { get; private set; }

		public void SetNumberInvoke(Actions.Action action)
		{
			this.OnSend(action);
		}

		protected internal override object GetState() => this.Number;

		protected override bool HandleAcceptedAction(Actions.Action action)
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

		protected override void ReceiveTracingBullet(TracingBulletAction action)
		{
			throw new NotImplementedException();
		}

		

		protected internal override void BulletTaken(List<Stack<Address>> ways, Actions.Action targetAction)
		{
			throw new NotImplementedException();
		}

		protected override List<TracingElement> GetAllowedForForwording(TracingBulletAction action, ref List<TracingElement> queue)
		{
			throw new NotImplementedException();
		}
	}
}