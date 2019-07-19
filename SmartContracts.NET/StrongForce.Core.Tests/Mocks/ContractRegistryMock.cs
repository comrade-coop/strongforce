namespace StrongForce.Core.Tests.Mocks
{
	public class ContractRegistryMock : ContractRegistry
	{
		public ContractRegistryMock(object initialState = null)
			: base(initialState)
		{
		}

		public bool HandleSendAction(Action action, Address sender)
		{
			action.Origin = sender;
			action.Sender = sender;
			return this.HandleAction(action);
		}
	}
}