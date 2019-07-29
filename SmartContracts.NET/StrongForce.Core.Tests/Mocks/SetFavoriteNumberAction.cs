namespace StrongForce.Core.Tests.Mocks
{
	public class SetFavoriteNumberAction : Action
	{
		public SetFavoriteNumberAction(Address origin, Address target, int number)
			: base(origin, target)
		{
			this.Number = number;
		}

		public int Number { get; }
	}
}