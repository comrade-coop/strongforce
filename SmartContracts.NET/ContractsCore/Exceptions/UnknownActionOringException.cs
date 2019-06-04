using System;

namespace StrongForce.Core.Exceptions
{
	class UnknownActionOringException : Exception
	{
		public Action Action { get; private set; }

		public UnknownActionOringException(Action action)
			: base($"Action \"{action.GetType()}-{action.GetHashCode()}\" has origin set by unappropriate way \"origin - {action.Origin}\" ")
		{
			this.Action = action;
		}
	}
}