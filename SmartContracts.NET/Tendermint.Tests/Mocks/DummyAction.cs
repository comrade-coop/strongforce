using StrongForce.Core;
using Action = StrongForce.Core.Action;

namespace Tendermint.Tests.Mocks
{
	public class DummyAction : Action
	{
		public DummyAction(Address target, Action forwarded = null, object objectProperty = null)
			: base(target)
		{
			// TODO: Uncomment this and fix tests
			// this.Sender = Address.Null();
			this.NextAction = forwarded;
			this.ObjectProperty = objectProperty;
			this.ObjectProperty = objectProperty;
		}

		public Action NextAction { get; }

		public object ObjectProperty { get; }
	}
}