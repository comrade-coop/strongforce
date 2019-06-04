using StrongForce.Core;
using Action = StrongForce.Core.Action;

namespace Tendermint.Tests.Mocks
{
	public class DummyAction : Action
	{
		public DummyAction(string hash, Address target, Action forwarded = null, object objectProperty = null)
			: base(hash, target)
		{
			this.NextAction = forwarded;
			this.ObjectProperty = objectProperty;
		}

		public Action NextAction { get; private set; }

		public object ObjectProperty { get; private set; }
	}
}