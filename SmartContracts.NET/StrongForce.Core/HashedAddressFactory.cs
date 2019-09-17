using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using StrongForce.Core.Extensions;

namespace StrongForce.Core
{
	public class HashedAddressFactory : BaseAddressFactory
	{
		public int AddressLength { get; private set; } = 20;

		public HashAlgorithm HashAlgorithm { get; private set; } = new SHA256Managed();

		public ulong AddressNonce { get; private set; } = 1;

		public override Address CreateAddress()
		{
			this.AddressNonce += 1;
			var data = BitConverter.GetBytes(this.AddressNonce);
			var addressData = this.HashAlgorithm.ComputeHash(data).Take(this.AddressLength).ToArray();
			return new Address(addressData);
		}

		public override IDictionary<string, object> GetState()
		{
			var state = base.GetState();

			state.Set("AddressLength", this.AddressLength);
			state.Set("HashAlgorithm", this.HashAlgorithm.GetType().AssemblyQualifiedName);
			state.Set("AddressNonce", this.AddressNonce.ToString());

			return state;
		}

		protected override void SetState(IDictionary<string, object> state)
		{
			this.AddressLength = state.Get<int>("AddressLength");
			this.AddressNonce = ulong.Parse(state.Get<string>("AddressNonce"));
			var hashAlgorithmType = Type.GetType(state.Get<string>("HashAlgorithm"));
			this.HashAlgorithm = (HashAlgorithm)Activator.CreateInstance(hashAlgorithmType);

			base.SetState(state);
		}
	}
}