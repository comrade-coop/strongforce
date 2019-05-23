using System;
using System.Collections.Generic;
using System.Text;
using ContractsCore.Permissions;
using ContractsCore.Permissions.WildCard;
using Xunit;

namespace ContractsCore.Tests
{
	public class WildCardTest
	{
		private readonly IAddressFactory addressFactory;

		public WildCardTest()
		{
			this.addressFactory = new RandomAddressFactory();
		}

		[Fact]
		public void AddWildCard_ReturnsTrue()
		{
			WildCardSet wildCardSet = new WildCardSet();
			IWildCard any = new AnyWildCard();
			wildCardSet.AddWildCard(any);
			Assert.True(wildCardSet.Contains(any));
		}

		[Fact]
		public void AddAddress_ReturnsTrue()
		{
			WildCardSet wildCardSet = new WildCardSet();
			Address addr = this.addressFactory.Create();
			wildCardSet.AddWildCard(addr);
			Assert.True(wildCardSet.IsMember(addr));
		}

		[Fact]
		public void HasPermission_WhenNotAdded_ReturnsTrue()
		{
			WildCardSet wildCardSet = new WildCardSet();
			Address addr = this.addressFactory.Create();
			Assert.False(wildCardSet.IsMember(addr));
		}

		[Fact]
		public void HasAddress_WhenMunltipleAddresses_ReturnsTrue()
		{
			WildCardSet wildCardSet = new WildCardSet();
			Address addr1 = this.addressFactory.Create();
			Address addr2 = this.addressFactory.Create();
			Address addr3 = this.addressFactory.Create();
			wildCardSet.AddWildCard(addr1);
			wildCardSet.AddWildCard(addr2);
			wildCardSet.AddWildCard(addr3);
			Assert.True(wildCardSet.IsMember(addr1));
			Assert.True(wildCardSet.IsMember(addr2));
			Assert.True(wildCardSet.IsMember(addr3));
		}

		[Fact]
		public void HasPermission_WhenAnyWildCard_ReturnsTrue()
		{
			WildCardSet wildCardSet = new WildCardSet();
			Address addr = this.addressFactory.Create();
			IWildCard any = new AnyWildCard();
			wildCardSet.AddWildCard(any);
			Assert.True(wildCardSet.IsMember(addr));
		}
	}
}