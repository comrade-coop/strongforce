using System;
using System.Collections.Generic;
using System.Linq;

namespace StrongForce.Core.Permissions
{
	public class AccessControlList
	{
		private readonly IDictionary<Permission, IDictionary<WildCard, HashSet<WildCard>>> permissionsToWildCardToWildCard;

		public AccessControlList(IDictionary<Permission, IDictionary<WildCard, HashSet<WildCard>>> initialWildCards)
		{
			this.permissionsToWildCardToWildCard = initialWildCards;
		}

		public AccessControlList()
			: this(new SortedDictionary<Permission, IDictionary<WildCard, HashSet<WildCard>>>())
		{
		}

		public List<Address> GetPermittedAddresses(Permission permission, Address target)
		{
			IDictionary<WildCard, HashSet<WildCard>> wildCardsSets = this.permissionsToWildCardToWildCard[permission];
			List<Address> members = new List<Address>();
			foreach (var cardSet in wildCardsSets)
			{
				foreach (var card in cardSet.Value)
				{
					if (card.IsMember(target))
					{
						members = members.Union(cardSet.Key.GetMembers()).ToList();
					}
				}
			}

			return members;
			throw new NotImplementedException();
		}

		public bool HasPermission(Address sender, Permission permission, Address receiver)
		{
			if (!this.permissionsToWildCardToWildCard.ContainsKey(permission))
			{
				return false;
			}

			var permissionCheck =
				from couple in this.permissionsToWildCardToWildCard[permission]
				where couple.Key.IsMember(sender)
				from set in couple.Value
				where set.IsMember(receiver)
				select set;

			return permissionCheck.Count() > 0;
		}

		public bool AddPermission(WildCard sender, Permission permission, WildCard receiver)
		{
			if (sender == null || receiver == null || permission == null)
			{
				return false;
			}

			if (!this.permissionsToWildCardToWildCard.ContainsKey(permission))
			{
				this.permissionsToWildCardToWildCard[permission] = new SortedDictionary<WildCard, HashSet<WildCard>>() { { sender, new HashSet<WildCard>() { receiver } } };
				return true;
			}
			else
			{
				if (this.permissionsToWildCardToWildCard[permission].ContainsKey(sender))
				{
					return this.permissionsToWildCardToWildCard[permission][sender].Add(receiver);
				}
				else
				{
					this.permissionsToWildCardToWildCard[permission].Add(sender, new HashSet<WildCard>() { receiver });
				}
			}

			return true;
		}

		public bool AddPermission(Address sender, Permission permission, Address receiver)
		{
			if (sender == null || receiver == null || permission == null)
			{
				return false;
			}

			var senderCard = new AddressWildCard() { sender };
			var receiverCard = new AddressWildCard() { receiver };

			return this.AddPermission(senderCard, permission, receiverCard);
		}

		public bool UpdatePermission(WildCard oldSender, WildCard oldReceiver, Permission permission, WildCard newSender, WildCard newReceiver)
		{
			if (oldSender == null || newSender == null || newReceiver == null || permission == null
				|| !this.permissionsToWildCardToWildCard.ContainsKey(permission)
				|| !this.permissionsToWildCardToWildCard[permission].ContainsKey(oldSender))
			{
				return false;
			}

			if (this.permissionsToWildCardToWildCard[permission][oldSender].Remove(oldReceiver))
			{
				this.AddPermission(newSender, permission, newReceiver);
			}

			return true;
		}

		public bool RemovePermission(WildCard sender, Permission permission, WildCard receiver)
		{
			if (sender == null || permission == null || !this.permissionsToWildCardToWildCard.ContainsKey(permission)
				|| !this.permissionsToWildCardToWildCard[permission].ContainsKey(sender))
			{
				return false;
			}

			return this.permissionsToWildCardToWildCard[permission][sender].Remove(receiver);
		}
	}
}