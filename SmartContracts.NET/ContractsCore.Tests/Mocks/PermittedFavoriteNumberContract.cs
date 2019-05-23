using System.Collections.Generic;
using System.Linq;
using ContractsCore.Actions;
using ContractsCore.Contracts;
using ContractsCore.Permissions;

namespace ContractsCore.Tests.Mocks
{
	public class PermittedFavoriteNumberContract : AclPermittedContract
	{
		public PermittedFavoriteNumberContract(Address permissionManager)
			: base(permissionManager)
		{
		}

		public PermittedFavoriteNumberContract(Address permissionManager, AccessControlList acl)
			: base(permissionManager, acl)
		{
		}

		public List<List<Address>> LastWays { get; } = new List<List<Address>>();

		public int Number { get; private set; }

		public bool GenerateActionAndFindPath(Address target, int num)
		{
			var setNumberAction = new SetFavoriteNumberAction(target, num);
			var x = new List<TracingElement>();
			var trace = new TracingBulletAction(target, setNumberAction, this.BulletTaken, null, ref x);
			this.OnSend(trace);
			return true;
		}

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

		protected internal override void BulletTaken(List<Stack<Address>> ways, Action targetAction)
		{
			foreach (var stack in ways)
			{
				this.LastWays.Add(stack.ToList());
			}

			Address target = ways[0].Pop();
			this.OnForward(targetAction, target, ways[0]);
		}

		private void HandleSetNumberAction(SetFavoriteNumberAction favoriteNumberAction)
		{
			this.Number = favoriteNumberAction.Number;
		}
	}
}