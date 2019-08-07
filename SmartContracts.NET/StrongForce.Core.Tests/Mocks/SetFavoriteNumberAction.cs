namespace StrongForce.Core.Tests.Mocks
{
	public class SetFavoriteNumberAction : Action
	{
		public SetFavoriteNumberAction(Address target, int number, object objectProperty = null)
			: base(target)
		{
			this.Number = number;
			this.ObjectProperty = objectProperty;
		}

		public int Number { get; }

		public object ObjectProperty { get; }
	}
}