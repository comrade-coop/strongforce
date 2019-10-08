using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StrongForce.Core.Kits;
using Wetonomy.Achievements;

namespace StrongForce.Integrations.Cosmos.IntegrationTest
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			this.Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
			services.AddSingleton<IOptions<StrongForceService.StrongForceServiceSettings>>((_) => new OptionsWrapper<StrongForceService.StrongForceServiceSettings>(new StrongForceService.StrongForceServiceSettings()
			{
				InitialKitType = typeof(AchievementsKit),
				InitialKitPayload = new Dictionary<string, object>()
				{
					{ "User", null },
					{ "ExchangeRateNumerator", 1 },
					{ "ExchangeRateDenominator", 2 },
				},
			}));
			services.AddSingleton<IHostedService, StrongForceService>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseMvc();
		}
	}
}