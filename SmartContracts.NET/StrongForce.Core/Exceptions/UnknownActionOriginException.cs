using System;

namespace StrongForce.Core.Exceptions
{
	public class UnknownActionOriginException : Exception
	{
		public UnknownActionOriginException(Action action)
			: base(
				$"Action \"{action.GetType()}-{action.GetHashCode()}\" has origin set by unappropriate path \"origin - {action.Origin}\" ")
		{
			this.Action = action;
		}

		public Action Action { get; private set; }
	}
}