using StrongForce.Core;
using Action = StrongForce.Core.Action;

namespace Tendermint.Tests.Mocks
{
	public class DummyAction : Action
	{
		public DummyAction(Address target, Action forwarded = null, object objectProperty = null)
			: base(target)
		{
			this.NextAction = forwarded;
			this.ObjectProperty = objectProperty;
			this.ObjectProperty = objectProperty;
		}

		public Action NextAction { get; }

		public object ObjectProperty { get; }
	}
}