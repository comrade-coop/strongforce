using System.Collections.Generic;
using System.Linq;
using StrongForce.Core;
using StrongForce.Core.Extensions;
using StrongForce.Core.Kits;
using StrongForce.Core.Permissions;

namespace StrongForce.Core.Tests.Mocks
{
	public class FavoriteNumberKit : Kit
	{
		public FavoriteNumberKit(int favoriteContractsCount)
		{
			this.FavoriteContractsCount = favoriteContractsCount;
		}

		public int FavoriteContractsCount { get; }

		public override Address Instantiate(Address initialManager)
		{
			var address = initialManager;

			for (int i = 0; i < this.FavoriteContractsCount; i++)
			{
				address = this.CreateContract<CreatorContract>(
					new Dictionary<string, object>() { { "Admin", initialManager.AsString() } });
			}

			return address;
		}
	}
}