using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Strongforce;
using StrongForce.Core;
using StrongForce.Core.Kits;
using StrongForce.Core.Serialization;

namespace StrongForce.Integrations.Cosmos
{
	public class StrongForceService : IHostedService
	{
		private readonly StrongForceServiceSettings settings;

		private ILogger<CosmosIntegrationFacade> logger;
		private Server server;

		public StrongForceService()
		{
			this.settings = new StrongForceServiceSettings();
			this.logger = null;
		}

		public StrongForceService(ILogger<CosmosIntegrationFacade> logger, Type initialKitType, IDictionary<string, object> initialKitPayload = null)
		{
			this.settings = new StrongForceServiceSettings();
			this.logger = logger;
		}

		public StrongForceService(ILogger<CosmosIntegrationFacade> logger, IOptions<StrongForceServiceSettings> options)
		{
			this.settings = options.Value;
			this.logger = logger;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			var initialKit = StatefulObject.Create(this.settings.InitialKitType, this.settings.InitialKitPayload);
			var facade = new CosmosIntegrationFacade(this.logger, StrongForceSerialization.SerializeStatefulObject(initialKit));
			var registry = new ContractRegistry(facade, new RandomAddressFactory());

			this.server = new Server
			{
				Services = { Strongforce.StrongForce.BindService(facade) },
				Ports = { new ServerPort(this.settings.Hostname, this.settings.Port, ServerCredentials.Insecure) },
			};
			this.server.Start();
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return this.server.ShutdownAsync();
		}

		public class StrongForceServiceSettings
		{
			public string Hostname { get; set; } = "localhost";

			public int Port { get; set; } = 8989;

			public Type InitialKitType { get; set; } = typeof(KitContract);

			public IDictionary<string, object> InitialKitPayload { get; set; } = new Dictionary<string, object>();
		}
	}
}