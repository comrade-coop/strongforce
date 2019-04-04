using ContractsCore.Actions;

namespace ContractsCore.Tests.Mocks
{
	public class SetFavoriteNumberAction : Action
	{
		public SetFavoriteNumberAction(string hash, Address origin, Address sender, Address target, int number)
			: base(hash, origin, sender, target)
		{
			this.Number = number;
		}

		public int Number { get; }
	}
}