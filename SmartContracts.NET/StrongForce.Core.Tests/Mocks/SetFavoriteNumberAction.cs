namespace StrongForce.Core.Tests.Mocks
{
	public class SetFavoriteNumberAction : Action
	{
		public SetFavoriteNumberAction(string hash, Address target, int number)
			: base(hash, target)
		{
			this.Number = number;
		}

		public int Number { get; }
	}
}