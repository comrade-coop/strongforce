using System;
using System.Collections.Generic;
using System.Linq;
using StrongForce.Core;
using StrongForce.Core.Extensions;
using StrongForce.Core.Permissions;

namespace StrongForce.Core.Tests.Mocks
{
	public class CatchingContract : BaseContract
	{
		public CatchingContract()
		{
		}

		protected override void CheckPermissions(Message message)
		{
		}

		protected override void HandleMessage(Message message)
		{
		}

		protected override void HandleForwardMessage(ForwardMessage message)
		{
			try
			{
				this.ForwardMessage(message.ForwardId);
			}
			catch (Exception)
			{
				// Silently consume
			}
		}
	}
}