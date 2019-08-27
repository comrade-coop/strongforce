using System;
using System.Collections.Generic;
using StrongForce.Core.Extensions;

namespace StrongForce.Core.Kits
{
	public class KitContract : Contract
	{
		public static Address DefaultAddress { get; } = new Address(new byte[] { 0 });

		public bool Instantiated { get; set; } = false;

		public override IDictionary<string, object> GetState()
		{
			var state = base.GetState();

			state.Add("Instantiated", this.Instantiated);

			return state;
		}

		protected override void SetState(IDictionary<string, object> state)
		{
			base.SetState(state);

			this.Instantiated = state.Get<bool>("Instantiated");
		}

		protected override void Initialize(IDictionary<string, object> payload)
		{
			if (payload.ContainsKey("User"))
			{
				this.Acl.AddPermission(
					payload.GetAddress("User"),
					InstantiateKitAction.Type,
					this.Address);
			}

			base.Initialize(payload);
		}

		protected override void HandleMessage(Message message)
		{
			switch (message.Type)
			{
				case InstantiateKitAction.Type:
					this.HandleInstantiateKitMessage(message.Sender);
					return;

				default:
					base.HandleMessage(message);
					return;
			}
		}

		private void HandleInstantiateKitMessage(Address initialManager)
		{
			if (this.Instantiated)
			{
				throw new InvalidOperationException("Kit was already instantiated");
			}

			this.Instantiated = true;

			this.Instantiate(initialManager);
		}

		protected virtual void Instantiate(Address initialManager)
		{
		}
	}
}