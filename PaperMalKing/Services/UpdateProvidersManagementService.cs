using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaperMalKing.UpdateProviders.Base;

namespace PaperMalKing.Services
{
	public sealed class UpdateProvidersManagementService
	{
		private readonly ILogger<UpdateProvidersManagementService> _logger;
		private readonly List<IUpdateProvider> _providers = new List<IUpdateProvider>();

		public IReadOnlyList<IUpdateProvider> Providers => this._providers;

		public UpdateProvidersManagementService(ILogger<UpdateProvidersManagementService> logger)
		{
			this._logger = logger;
		}

		public static void ConfigureProviders(IConfiguration configuration, IServiceCollection services)
		{
			foreach (var updateProvider in Assembly.GetExecutingAssembly().GetTypes()
												   .Where(t => t.GetInterfaces().Contains(typeof(IUpdateProvider))))
			{
				updateProvider.GetMethod("Configure")?.Invoke(null, new object[]{configuration, services});
			}
		}

		public void CreateProviders(IServiceProvider services)
		{
			if (this._providers.Count != 0)
			{
				this._logger.LogError("Providers are already set");
				return;
			}

			this._providers.AddRange(services.GetServices<IUpdateProvider>());
		}

		public Task[] StartUpdateChecking(CancellationToken token = default)
		{
			var tasks = new Task[this._providers.Count];
			for (var i = 0; i < this._providers.Count; i++)
			{
				tasks[i] = this._providers[i].GetUpdates(token);
			}

			return tasks;
		}
	}
}