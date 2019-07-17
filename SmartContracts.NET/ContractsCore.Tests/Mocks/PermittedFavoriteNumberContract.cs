
using ContractsCore.Actions;
using ContractsCore.Contracts;
using ContractsCore.Permissions;
using System.Collections.Generic;
using System.Linq;

namespace ContractsCore.Tests.Mocks
{
	public class PermittedFavoriteNumberContract : AclPermittedContract
	{
		public PermittedFavoriteNumberContract(ContractRegistry registry, Address permissionManager)
			: base(registry, permissionManager)
		{
		}

		public PermittedFavoriteNumberContract(ContractRegistry registry, Address permissionManager, AccessControlList acl)
			: base(registry, permissionManager, acl)
		{
		}

		public int Number { get; private set; }

		public List<List<Address>> LastWays = new List<List<Address>>();

		protected override object GetState() => this.Number;

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

		public bool CheckPermission(object address, Permission permission, object target)
		{
			return this.Acl.HasPermission(address, permission, target);
		}

		public bool GenerateActionAndFindPath(Address target, int num)
		{
			var setNumberAction = new SetFavoriteNumberAction(string.Empty, target, num);
			var x = new List<TracingElement>();
			var trace = new TracingBulletAction(string.Empty, target, setNumberAction, this.BulletTaken, null, ref x);
			this.OnSend(trace);
			return true;
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
	}
}