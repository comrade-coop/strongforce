using System.Runtime.Serialization;

namespace StrongForce.Core
{
	public class ForwardAction : Action
	{
		public ForwardAction(Address[] targets, Action finalAction)
			: base(targets[0])
		{
			var wrapped = finalAction;

			// i > 0 is not a typo.
			for (int i = targets.Length - 1; i > 0; i--)
			{
				wrapped = new ForwardAction(targets[i], wrapped);
			}

			this.NextAction = wrapped;
			this.FinalAction = (this.NextAction as ForwardAction)?.FinalAction ?? this.NextAction;
		}

		public ForwardAction(Address target, Action finalAction)
			: this(new Address[] { target }, finalAction)
		{
		}

		public Action NextAction { get; }

		public Action FinalAction { get; }
	}
}