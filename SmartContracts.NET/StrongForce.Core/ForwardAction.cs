using System.Runtime.Serialization;

namespace StrongForce.Core
{
	public class ForwardAction : Action
	{
		public ForwardAction(Address target, Action nextAction)
			: base(target)
		{
			this.NextAction = nextAction;
			this.NextAction.ConfigureSender(target);
		}

		public ForwardAction(Address[] targets, Action finalAction)
			: base(targets[0])
		{
			var wrapped = finalAction;

			// i > 0 is not a typo.
			for (int i = targets.Length - 1; i > 0; i--)
			{
				wrapped.ConfigureSender(targets[i]);
				wrapped = new ForwardAction(targets[i], wrapped);
			}

			this.NextAction = wrapped;
			this.NextAction.ConfigureSender(targets[0]);
		}

		public Action NextAction { get; }

		public Action FinalAction { get => (this.NextAction as ForwardAction)?.FinalAction ?? this.NextAction; }

		protected override void AfterConfigureOrigin()
		{
			this.NextAction.ConfigureOrigin(this.Origin);
			base.AfterConfigureOrigin();
		}
	}
}