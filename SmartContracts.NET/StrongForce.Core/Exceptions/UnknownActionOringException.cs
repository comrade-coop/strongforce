using System;

namespace StrongForce.Core.Exceptions
{
	public class UnknownActionOringException : Exception
	{
		public UnknownActionOringException(Action action)
			: base($"Action \"{action.GetType()}-{action.GetHashCode()}\" has origin set by unappropriate way \"origin - {action.Origin}\" ")
		{
			this.Action = action;
		}

		public Action Action { get; private set; }
	}
}