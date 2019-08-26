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

namespace StrongForce.Integrations.Cosmos
{
	public class StrongForceService : IHostedService
	{
		private readonly StrongForceServiceSettings settings;

		private ILogger<CosmosIntegrationFacade> logger;
		private Kit initialKit;
		private Server server;

		public StrongForceService()
		{
			this.settings = new StrongForceServiceSettings();
			this.logger = null;
			this.initialKit = null;
		}

		public StrongForceService(ILogger<CosmosIntegrationFacade> logger, Kit initialKit)
		{
			this.settings = new StrongForceServiceSettings();
			this.logger = logger;
			this.initialKit = initialKit;
		}

		public StrongForceService(ILogger<CosmosIntegrationFacade> logger, IOptions<StrongForceServiceSettings> options, Kit initialKit)
		{
			this.settings = options.Value;
			this.logger = logger;
			this.initialKit = initialKit;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			var facade = new CosmosIntegrationFacade(this.logger, this.initialKit);
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
		}
	}
}