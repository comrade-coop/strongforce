using ContractsCore.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ContractsCore.Permissions
{
	public class WildCardSet: IWildCard, IComparable<WildCardSet>
	{
		public HashSet<IWildCard> WildCards;
		private AddressWildCard AddressCard;

		public WildCardSet(AddressWildCard addressCard = null, HashSet<IWildCard> wildCards = null)
		{
			if (addressCard == null)
			{
				this.AddressCard = new AddressWildCard();
			}
			else
			{
				this.AddressCard = addressCard;
			}

			if (wildCards == null)
			{
				this.WildCards = new HashSet<IWildCard>();
			}
			else
			{
				this.WildCards = wildCards;
			}

			WildCards.Add(AddressCard);
		}

		public static WildCardSet FromAddress(Address address)
		{
			WildCardSet set = new WildCardSet();
			set.AddAddress(address);
			return set;
		}

		public static WildCardSet FromObject(object obj)
		{
			if (typeof(WildCardSet).IsAssignableFrom(obj.GetType()))
			{
				return obj as WildCardSet;
			}
			else if (typeof(Address).IsAssignableFrom(obj.GetType()))
			{
				return FromAddress(obj as Address);
			}
			else
			{
				throw new UnsupportedTypeExeption(null, obj, "WildCardSet FormObject()");
			}
		}

		public bool AddWildCard(object card)
		{
			if (typeof(IWildCard).IsAssignableFrom(card.GetType()))
			{
				return this.WildCards.Add(card as IWildCard);
			}
			else if (typeof(Address).IsAssignableFrom(card.GetType()))
			{
				return this.AddAddress(card as Address);
			}
			else
			{
				throw new UnsupportedTypeExeption(this, card, "AddWildCard()");
			}
		}

		public bool RemoveWildCard(object card)
		{
			if (typeof(IWildCard).IsAssignableFrom(card.GetType()))
			{
				return this.WildCards.Remove(card as IWildCard);
			}
			else
			{
				return this.RemoveAddress(card as Address);
			}
		}

		public bool AddAddress(Address addr)
		{
			return this.AddressCard.Add(addr);
		}

		public bool RemoveAddress(Address addr)
		{
			return this.AddressCard.Remove(addr);
		}

		public bool IsMember(object member)
		{
			if (typeof(IWildCard).IsAssignableFrom(member.GetType()))
			{
				return this.Contains(member as IWildCard);
			}
			else if (typeof(Address).IsAssignableFrom(member.GetType()))
			{
				return this.IsMember(member as Address);
			}
			else
			{
				throw new UnsupportedTypeExeption(this, member, "IsMember()");
			}
		}

		public bool IsMember(Address member)
		{
			foreach (var card in WildCards)
			{
				if (card.IsMember(member)) return true;
			}

			return false;
		}

		public bool Contains(IWildCard card)
		{
			if (typeof(WildCardSet).IsAssignableFrom(card.GetType()))
			{
				return this.Equal(card);
			}
			foreach (var wildCard in this.WildCards)
			{
				if (wildCard.Equal(card))
				{
					return true;
				}
			}

			return false;
		}

		public List<Address> GetMembers()
		{
			List<Address> members = new List<Address>();
			foreach (var card in WildCards)
			{
				members = members.Union(card.GetMembers()).ToList();
			}

			return members;
		}

		public int CompareTo(WildCardSet other)
		{
			if (other == null) return 1;
			if (this.WildCards.Count != other.WildCards.Count)
			{
				return this.WildCards.Count.ToString().CompareTo(other.WildCards.Count.ToString());
			}

			/*if (this.WildCards.SetEquals(other.WildCards))
			{
				return 0;
			}*/
			foreach (var card in this.WildCards)
			{
				if (!other.Contains(card))
				{
					return -1;
				}
			}

			return 0;
		}

		public bool Equal(IWildCard member)
		{
			foreach (var memb in member.GetMembers())
			{
				if (!this.IsMember(memb)) return false;
			}

			return true;
		}
	}
}