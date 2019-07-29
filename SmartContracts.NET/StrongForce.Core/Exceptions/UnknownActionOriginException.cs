using System;

namespace StrongForce.Core.Exceptions
{
	public class UnknownActionOriginException : Exception
	{
		public UnknownActionOriginException(Action action, Address expectedFrom)
			: base(
			$"Action \"{action.GetType()}-{action.GetHashCode()}\" has inapproperiate origin {action.Origin}, expected {expectedFrom} ")
		{
			this.Action = action;
			this.ExpectedOrigin = expectedFrom;
		}

		public Action Action { get; }

		public Address ExpectedOrigin { get; }
	}
}