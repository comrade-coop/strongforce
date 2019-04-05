using System;

namespace ContractsCore.Permissions
{
	public class Permission
	{
		public Permission(Type type)
		{
			this.Type = type;
		}

		public Type Type { get; }

		public override string ToString()
		{
			return this.Type.Name;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || obj.GetType() != typeof(Permission))
			{
				return false;
			}

			return this.Equals((Permission) obj);
		}

		public override int GetHashCode()
		{
			return this.Type != null ? this.Type.GetHashCode() : 0;
		}

		protected bool Equals(Permission other)
		{
			return this.Type == other.Type;
		}
	}
}