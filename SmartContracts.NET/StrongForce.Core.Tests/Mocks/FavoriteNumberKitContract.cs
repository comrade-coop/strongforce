using System.Collections.Generic;
using System.Linq;
using StrongForce.Core;
using StrongForce.Core.Extensions;
using StrongForce.Core.Kits;
using StrongForce.Core.Permissions;

namespace StrongForce.Core.Tests.Mocks
{
	public class FavoriteNumberKitContract : KitContract
	{
		protected int FavoriteContractsCount { get; set; } = 1;

		public override IDictionary<string, object> GetState()
		{
			var state = base.GetState();

			state.Add("FavoriteContractsCount", this.FavoriteContractsCount);

			return state;
		}

		protected override void SetState(IDictionary<string, object> state)
		{
			this.FavoriteContractsCount = state.Get<int>("FavoriteContractsCount");

			base.SetState(state);
		}

		protected override void Instantiate(Address initialManager)
		{
			var address = initialManager;

			for (int i = 0; i < this.FavoriteContractsCount; i++)
			{
				address = this.CreateContract<CreatorContract>(
					new Dictionary<string, object>() { { "Admin", initialManager?.ToString() } });
			}
		}
	}
}