using System.Collections.Generic;
using System.Linq;
using StrongForce.Core.Permissions;
using StrongForce.Core.Permissions.Actions;

namespace StrongForce.Core.Tests.Mocks
{
	public class PermittedFavoriteNumberContract : AclPermittedContract
	{
		public PermittedFavoriteNumberContract(Address address, Address permissionManager)
			: this(address, permissionManager, new AccessControlList())
		{
		}

		public PermittedFavoriteNumberContract(Address address, Address permissionManager, AccessControlList acl)
			: base(address, permissionManager, acl)
		{
			this.LastPaths = new List<List<Address>>();
		}

		public int Number { get; private set; }

		public List<List<Address>> LastPaths { get; }

		public bool CheckPermission(Address address, Permission permission, Address target)
		{
			return this.Acl.HasPermission(address, permission, target);
		}

		public bool GenerateActionAndFindPath(Address target, int num)
		{
			var setNumberAction = new SetFavoriteNumberAction(target, num);
			var trace = new TracingBulletAction(target, setNumberAction, this.BulletTaken, null);
			this.SendAction(trace);
			return true;
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
					return base.HandleAction(action);
			}
		}

		protected override void BulletTaken(List<Stack<Address>> paths, Action targetAction)
		{
			foreach (var stack in paths)
			{
				this.LastPaths.Add(stack.ToList());
			}

			Address target = paths[0].Pop();
			var forward = new ForwardAction(
				target,
				targetAction,
				paths[0]);
			this.SendAction(forward);
		}

		private void HandleSetNumberAction(SetFavoriteNumberAction favoriteNumberAction)
		{
			this.Number = favoriteNumberAction.Number;
		}
	}
}