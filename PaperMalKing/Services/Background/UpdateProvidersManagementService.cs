using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.Services.Background
{
	public sealed class UpdateProvidersManagementService : BackgroundService
	{
		private readonly ILogger<UpdateProvidersManagementService> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly UpdateProvidersConfigurationService _updateProvidersConfigurationService;
		

		public UpdateProvidersManagementService(ILogger<UpdateProvidersManagementService> logger, IServiceProvider serviceProvider,
												UpdateProvidersConfigurationService updateProvidersConfigurationService)
		{
			this._logger = logger;

			this._logger.LogTrace($"Building {nameof(UpdateProvidersManagementService)}");
			this._serviceProvider = serviceProvider;
			this._updateProvidersConfigurationService = updateProvidersConfigurationService;
			this._logger.LogTrace($"Built {nameof(UpdateProvidersManagementService)}");
		}

		/// <inheritdoc />
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			this._logger.LogInformation("Starting to wait for shutdown for cancelling update providers checking for updates");
			await Task.Delay(Timeout.Infinite, stoppingToken);
			var tasks = new Task[this._updateProvidersConfigurationService.Providers.Count];
			var providers = this._updateProvidersConfigurationService.Providers.Values.ToArray();
			for (var i = 0; i < providers.Length; i++)
			{
				var provider = providers[i];
				tasks[i] = provider.TriggerStoppingAsync();
			}

			await Task.WhenAll(tasks);
		}
	}
}