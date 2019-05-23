using ContractsCore.Contracts;

namespace ContractsCore
{
	public interface IContractRegistry
	{
		void RegisterContract(Contract contract);

		T GetContract<T>(Address address)
			where T : Contract;
	}
}