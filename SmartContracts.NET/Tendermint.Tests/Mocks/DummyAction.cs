using StrongForce.Core;
using Action = StrongForce.Core.Action;

namespace Tendermint.Tests.Mocks
{
	public class DummyAction : Action
	{
		public DummyAction(Address target, Action forwarded = null, object objectProperty = null)
			: base(target)
		{
			this.Sender = Address.Null();
			this.NextAction = forwarded;
			this.ObjectProperty = objectProperty;
		}

		public Action NextAction { get; private set; }

		public object ObjectProperty { get; private set; }
	}
}