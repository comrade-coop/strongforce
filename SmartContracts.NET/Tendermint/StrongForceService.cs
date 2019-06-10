using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Strongforce;

namespace Tendermint
{
	public class StrongForceService : IHostedService
	{
		private readonly StrongForceServiceSettings settings;

		private Server server;

		public StrongForceService()
		{
			this.settings = new StrongForceServiceSettings();
		}

		public StrongForceService(IOptions<StrongForceServiceSettings> options)
		{
			this.settings = options.Value;
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			this.server = new Server
			{
				Services = { StrongForce.BindService(new StrongForceServer()) },
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