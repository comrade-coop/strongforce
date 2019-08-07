using StrongForce.Core;
using Action = StrongForce.Core.Action;

namespace StrongForce.Integrations.Cosmos.Tests.Mocks
{
	public class DummyAction : Action
	{
		public DummyAction(Address target, Action forwarded = null)
			: base(target)
		{
			this.NextAction = forwarded;
		}

		public Action NextAction { get; }
	}
}