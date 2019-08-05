using System;

namespace StrongForce.Core.Tests.Mocks
{
	public class CreateContractAction : Action
	{
		public CreateContractAction(Address target, Type contractType)
			: base(target)
		{
			this.ContractType = contractType;
		}

		public Type ContractType { get; }
	}
}