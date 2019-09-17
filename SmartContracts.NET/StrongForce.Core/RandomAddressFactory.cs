using System;
using System.Collections.Generic;
using StrongForce.Core.Extensions;

namespace StrongForce.Core
{
	public class RandomAddressFactory : BaseAddressFactory
	{
		private readonly Random random = new Random();

		public int AddressLength { get; private set; } = 20;

		public override Address CreateAddress()
		{
			var addressValue = new byte[this.AddressLength];
			this.random.NextBytes(addressValue);
			return new Address(addressValue);
		}

		public override IDictionary<string, object> GetState()
		{
			var state = base.GetState();

			state.Set("AddressLength", this.AddressLength);

			return state;
		}

		protected override void SetState(IDictionary<string, object> state)
		{
			this.AddressLength = state.Get<int>("AddressLength");

			base.SetState(state);
		}
	}
}