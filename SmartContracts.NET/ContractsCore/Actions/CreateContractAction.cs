using System;

namespace ContractsCore.Actions
{
	public class CreateContractAction : Action
	{
		public CreateContractAction(string hash, Address target, Type contractType, object[] constructorArgs)
			: base(hash, target)
		{
			this.ContractType = contractType;
			this.ConstructorArgs = constructorArgs;
		}

		public Type ContractType { get; }

		public object[] ConstructorArgs { get; }

		public Address ContractAddress { get; internal set; }
	}
}