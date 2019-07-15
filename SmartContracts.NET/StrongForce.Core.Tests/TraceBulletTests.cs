using System.Collections.Generic;
using StrongForce.Core.Permissions;
using StrongForce.Core.Tests.Mocks;
using Xunit;

namespace StrongForce.Core.Tests
{
	public class TraceBulletTests
	{
		private readonly IAddressFactory addressFactory;
		private ContractRegistryMock registry;

		public TraceBulletTests()
		{
			this.addressFactory = new RandomAddressFactory();
			this.registry = new ContractRegistryMock();
		}

		[Fact]
		public void Receive_WhenPassedSetFavoriteNumberActionWithGrantedPermissions_SetsNumberCorrectly()
		{
			Address permissionManager = this.addressFactory.Create();
			Address[] addrs = new Address[10];
			PermittedFavoriteNumberContract[] contracts = new PermittedFavoriteNumberContract[10];
			for (int i = 0; i < 4; i++)
			{
				addrs[i] = this.addressFactory.Create();
				contracts[i] = new PermittedFavoriteNumberContract(addrs[i], permissionManager);
				this.registry.RegisterContract(contracts[i]);
				var addTracingPermissionAction = new AddPermissionAction(
					string.Empty, addrs[i], new Permission(typeof(TracingBulletAction)), new AnyWildCard(), new AnyWildCard());
				var addForwardingPermissionAction = new AddPermissionAction(
					string.Empty, addrs[i], new Permission(typeof(ForwardAction)), new AnyWildCard(), new AnyWildCard());
				Assert.True(this.registry.HandleSendAction(addTracingPermissionAction, permissionManager));
				Assert.True(this.registry.HandleSendAction(addForwardingPermissionAction, permissionManager));
			}

			for (int i = 1; i < 4; i++)
			{
				var addPermissionAction = new AddPermissionAction(string.Empty, addrs[i - 1], new Permission(typeof(SetFavoriteNumberAction)), addrs[i]);
				Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));
			}

			contracts[3].GenerateActionAndFindPath(addrs[0], 14);
			Assert.Equal(14, contracts[0].Number);
		}

		[Fact]
		public void Receive_WhenPassedSetFavoriteNumberActionWithGrantedPermissions_ReturnPath()
		{
			Address permissionManager = this.addressFactory.Create();
			Address[] addrs = new Address[10];
			PermittedFavoriteNumberContract[] contracts = new PermittedFavoriteNumberContract[10];
			for (int i = 0; i <= 3; i++)
			{
				addrs[i] = this.addressFactory.Create();
				contracts[i] = new PermittedFavoriteNumberContract(addrs[i], permissionManager);
				this.registry.RegisterContract(contracts[i]);
				var addTracingPermissionAction = new AddPermissionAction(
					string.Empty, addrs[i], new Permission(typeof(TracingBulletAction)), new AnyWildCard(), new AnyWildCard());
				var addForwardingPermissionAction = new AddPermissionAction(
					string.Empty, addrs[i], new Permission(typeof(ForwardAction)), new AnyWildCard(), new AnyWildCard());
				Assert.True(this.registry.HandleSendAction(addTracingPermissionAction, permissionManager));
				Assert.True(this.registry.HandleSendAction(addForwardingPermissionAction, permissionManager));
			}

			// Path 0-1-2-3
			for (int i = 1; i <= 3; i++)
			{
				var addPermissionAction = new AddPermissionAction(string.Empty, addrs[i - 1], new Permission(typeof(SetFavoriteNumberAction)), addrs[i]);
				Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));
			}

			contracts[3].GenerateActionAndFindPath(addrs[0], 14);
			var x = new List<List<Address>>
			{
				new List<Address>() { addrs[2], addrs[1], addrs[0] },
			};
			Assert.Equal(x, contracts[3].LastWays);
		}

		[Fact]
		public void Receive_WhenPassedSetFavoriteNumberActionWithGrantedPermissions2_ReturnPath()
		{
			Address permissionManager = this.addressFactory.Create();
			Address[] addrs = new Address[10];
			PermittedFavoriteNumberContract[] contracts = new PermittedFavoriteNumberContract[10];
			for (int i = 0; i <= 5; i++)
			{
				addrs[i] = this.addressFactory.Create();
				contracts[i] = new PermittedFavoriteNumberContract(addrs[i], permissionManager);
				this.registry.RegisterContract(contracts[i]);
				var addTracingPermissionAction = new AddPermissionAction(
					string.Empty, addrs[i], new Permission(typeof(TracingBulletAction)), new AnyWildCard(), new AnyWildCard());
				var addForwardingPermissionAction = new AddPermissionAction(
					string.Empty, addrs[i], new Permission(typeof(ForwardAction)), new AnyWildCard(), new AnyWildCard());
				Assert.True(this.registry.HandleSendAction(addTracingPermissionAction, permissionManager));
				Assert.True(this.registry.HandleSendAction(addForwardingPermissionAction, permissionManager));
			}

			// Path 0<-1<-2<-3<-4
			//          \   /
			//            5
			AddPermissionAction addPermissionAction;
			for (int i = 1; i <= 4; i++)
			{
				addPermissionAction = new AddPermissionAction(string.Empty, addrs[i - 1], new Permission(typeof(SetFavoriteNumberAction)), addrs[i]);
				Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));
			}

			addPermissionAction = new AddPermissionAction(string.Empty, addrs[5], new Permission(typeof(SetFavoriteNumberAction)), addrs[3]);
			Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));
			addPermissionAction = new AddPermissionAction(string.Empty, addrs[1], new Permission(typeof(SetFavoriteNumberAction)), addrs[5]);
			Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));

			contracts[4].GenerateActionAndFindPath(addrs[0], 14);

			var x = new List<List<Address>>
			{
				new List<Address>() { addrs[3], addrs[2], addrs[1], addrs[0] },
				new List<Address>() { addrs[3], addrs[5], addrs[1], addrs[0] },
			};
			Assert.Equal(x, contracts[4].LastWays);
		}

		[Fact]
		public void Receive_WhenPassedSetFavoriteNumberActionWithGrantedPermissions3_ReturnPath()
		{
			Address permissionManager = this.addressFactory.Create();
			Address[] addrs = new Address[10];
			PermittedFavoriteNumberContract[] contracts = new PermittedFavoriteNumberContract[10];
			for (int i = 0; i <= 8; i++)
			{
				addrs[i] = this.addressFactory.Create();
				contracts[i] = new PermittedFavoriteNumberContract(addrs[i], permissionManager);
				this.registry.RegisterContract(contracts[i]);
				var addTracingPermissionAction = new AddPermissionAction(
					string.Empty, addrs[i], new Permission(typeof(TracingBulletAction)), new AnyWildCard(), new AnyWildCard());
				var addForwardingPermissionAction = new AddPermissionAction(
					string.Empty, addrs[i], new Permission(typeof(ForwardAction)), new AnyWildCard(), new AnyWildCard());
				Assert.True(this.registry.HandleSendAction(addTracingPermissionAction, permissionManager));
				Assert.True(this.registry.HandleSendAction(addForwardingPermissionAction, permissionManager));
			}

			// Path 0<-1<-2<-3<-4<-5<-6
			//          \   /     /
			//            8 <--- 7
			AddPermissionAction addPermissionAction;
			for (int i = 1; i <= 6; i++)
			{
				addPermissionAction = new AddPermissionAction(string.Empty, addrs[i - 1], new Permission(typeof(SetFavoriteNumberAction)), addrs[i]);
				Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));
			}

			addPermissionAction = new AddPermissionAction(string.Empty, addrs[8], new Permission(typeof(SetFavoriteNumberAction)), addrs[3]);
			Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));
			addPermissionAction = new AddPermissionAction(string.Empty, addrs[1], new Permission(typeof(SetFavoriteNumberAction)), addrs[8]);
			Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));
			addPermissionAction = new AddPermissionAction(string.Empty, addrs[7], new Permission(typeof(SetFavoriteNumberAction)), addrs[5]);
			Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));
			addPermissionAction = new AddPermissionAction(string.Empty, addrs[8], new Permission(typeof(SetFavoriteNumberAction)), addrs[7]);
			Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));

			contracts[6].GenerateActionAndFindPath(addrs[0], 14);

			var x = new List<List<Address>>
			{
				new List<Address>() { addrs[5], addrs[7], addrs[8], addrs[1], addrs[0] },
				new List<Address>() { addrs[5], addrs[4], addrs[3], addrs[2], addrs[1], addrs[0] },
				new List<Address>() { addrs[5], addrs[4], addrs[3], addrs[8], addrs[1], addrs[0] },
			};
			Assert.Equal(x, contracts[6].LastWays);
		}

		[Fact]
		public void Receive_WhenPassedSetFavoriteNumberActionWithGrantedPermissions4_ReturnPath()
		{
			Address permissionManager = this.addressFactory.Create();
			Address[] addrs = new Address[10];
			PermittedFavoriteNumberContract[] contracts = new PermittedFavoriteNumberContract[10];
			for (int i = 0; i <= 8; i++)
			{
				addrs[i] = this.addressFactory.Create();
				contracts[i] = new PermittedFavoriteNumberContract(addrs[i], permissionManager);
				this.registry.RegisterContract(contracts[i]);
				var addTracingPermissionAction = new AddPermissionAction(
					string.Empty, addrs[i], new Permission(typeof(TracingBulletAction)), new AnyWildCard(), new AnyWildCard());
				var addForwardingPermissionAction = new AddPermissionAction(
					string.Empty, addrs[i], new Permission(typeof(ForwardAction)), new AnyWildCard(), new AnyWildCard());
				Assert.True(this.registry.HandleSendAction(addTracingPermissionAction, permissionManager));
				Assert.True(this.registry.HandleSendAction(addForwardingPermissionAction, permissionManager));
			}

			// Path 0<-1<-2<-3<-4<-5<-6
			//          \   //    /
			//            8 <--- 7
			AddPermissionAction addPermissionAction;
			for (int i = 1; i <= 6; i++)
			{
				addPermissionAction = new AddPermissionAction(string.Empty, addrs[i - 1], new Permission(typeof(SetFavoriteNumberAction)), addrs[i]);
				Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));
			}

			addPermissionAction = new AddPermissionAction(string.Empty, addrs[8], new Permission(typeof(SetFavoriteNumberAction)), addrs[3]);
			Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));
			addPermissionAction = new AddPermissionAction(string.Empty, addrs[3], new Permission(typeof(SetFavoriteNumberAction)), addrs[8]);
			Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));
			addPermissionAction = new AddPermissionAction(string.Empty, addrs[1], new Permission(typeof(SetFavoriteNumberAction)), addrs[8]);
			Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));
			addPermissionAction = new AddPermissionAction(string.Empty, addrs[7], new Permission(typeof(SetFavoriteNumberAction)), addrs[5]);
			Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));
			addPermissionAction = new AddPermissionAction(string.Empty, addrs[8], new Permission(typeof(SetFavoriteNumberAction)), addrs[7]);
			Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));

			contracts[6].GenerateActionAndFindPath(addrs[0], 14);

			var x = new List<List<Address>>
			{
				new List<Address>() { addrs[5], addrs[7], addrs[8], addrs[1], addrs[0] },
				new List<Address>() { addrs[5], addrs[4], addrs[3], addrs[2], addrs[1], addrs[0] },
				new List<Address>() { addrs[5], addrs[4], addrs[3], addrs[8], addrs[1], addrs[0] },
				new List<Address>() { addrs[5], addrs[7], addrs[8], addrs[3], addrs[2], addrs[1], addrs[0] },
			};
			Assert.Equal(x, contracts[6].LastWays);
		}

		[Fact]
		public void Receive_WhenPassedSetFavoriteNumberActionWithGrantedPermissions5_ReturnPath()
		{
			Address permissionManager = this.addressFactory.Create();
			Address[] addrs = new Address[15];
			PermittedFavoriteNumberContract[] contracts = new PermittedFavoriteNumberContract[15];
			for (int i = 0; i <= 11; i++)
			{
				addrs[i] = this.addressFactory.Create();
				contracts[i] = new PermittedFavoriteNumberContract(addrs[i], permissionManager);
				this.registry.RegisterContract(contracts[i]);
				var addTracingPermissionAction = new AddPermissionAction(
					string.Empty, addrs[i], new Permission(typeof(TracingBulletAction)), new AnyWildCard(), new AnyWildCard());
				var addForwardingPermissionAction = new AddPermissionAction(
					string.Empty, addrs[i], new Permission(typeof(ForwardAction)), new AnyWildCard(), new AnyWildCard());
				Assert.True(this.registry.HandleSendAction(addTracingPermissionAction, permissionManager));
				Assert.True(this.registry.HandleSendAction(addForwardingPermissionAction, permissionManager));
			}

			// Path 0<-1<-2<-3<-4<-5<-6<-7<-8
			//          \   /       \   /
			//            9           10
			AddPermissionAction addPermissionAction;
			for (int i = 1; i <= 8; i++)
			{
				addPermissionAction = new AddPermissionAction(string.Empty, addrs[i - 1], new Permission(typeof(SetFavoriteNumberAction)), addrs[i]);
				Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));
			}

			addPermissionAction = new AddPermissionAction(string.Empty, addrs[1], new Permission(typeof(SetFavoriteNumberAction)), addrs[9]);
			Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));
			addPermissionAction = new AddPermissionAction(string.Empty, addrs[9], new Permission(typeof(SetFavoriteNumberAction)), addrs[3]);
			Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));
			addPermissionAction = new AddPermissionAction(string.Empty, addrs[5], new Permission(typeof(SetFavoriteNumberAction)), addrs[10]);
			Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));
			addPermissionAction = new AddPermissionAction(string.Empty, addrs[10], new Permission(typeof(SetFavoriteNumberAction)), addrs[7]);
			Assert.True(this.registry.HandleSendAction(addPermissionAction, permissionManager));

			contracts[8].GenerateActionAndFindPath(addrs[0], 14);

			var x = new List<List<Address>>
			{
				new List<Address>() { addrs[7], addrs[6], addrs[5], addrs[4], addrs[3], addrs[2], addrs[1], addrs[0] },
				new List<Address>() { addrs[7], addrs[10], addrs[5], addrs[4], addrs[3], addrs[2], addrs[1], addrs[0] },
				new List<Address>() { addrs[7], addrs[6], addrs[5], addrs[4], addrs[3], addrs[9], addrs[1], addrs[0] },
				new List<Address>() { addrs[7], addrs[10], addrs[5], addrs[4], addrs[3], addrs[9], addrs[1], addrs[0] },
			};
			Assert.Equal(x, contracts[8].LastWays);
		}
	}
}