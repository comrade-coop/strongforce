using System;
using System.Runtime.Serialization;

namespace StrongForce.Core
{
	public class Action
	{
		public Action(Address target)
		{
			this.Target = target;
		}

		[IgnoreDataMember]
		public bool IsConfigured { get => this.Sender != null && this.Origin != null; }

		[IgnoreDataMember]
		public Address Origin { get; private set; }

		[IgnoreDataMember]
		public Address Sender { get; private set; }

		public Address Target { get; }

		public Action ConfigureOrigin(Address origin)
		{
			if (this.Origin != origin && this.Origin != null)
			{
				throw new InvalidOperationException($"Another origin is already configured for this {this.GetType()}");
			}

			this.Origin = origin;
			this.AfterConfigureOrigin();
			return this;
		}

		public Action ConfigureSender(Address sender)
		{
			if (this.Sender != sender && this.Sender != null)
			{
				throw new InvalidOperationException($"Another sender is already configured for this {this.GetType()}");
			}

			this.Sender = sender;
			this.AfterConfigureSender();
			return this;
		}

		public Action ConfigureSenderAndOrigin(Address senderAndOrigin)
		{
			return this.ConfigureSender(senderAndOrigin).ConfigureOrigin(senderAndOrigin);
		}

		protected virtual void AfterConfigureOrigin()
		{
		}

		protected virtual void AfterConfigureSender()
		{
		}
	}
}