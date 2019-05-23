using Action = ContractsCore.Actions.Action;

namespace ContractsCore.Tests.Mocks
{
	public class ContractRegistryMock : ContractRegistry
	{
		public ContractRegistryMock()
			: base(new RandomAddressFactory())
		{
		}

		public bool HandleSendAction(Action action, Address sender)
		{
			action.Origin = sender;
			action.Sender = sender;
			return this.HandleAction(action, action.Target);
		}
	}
}