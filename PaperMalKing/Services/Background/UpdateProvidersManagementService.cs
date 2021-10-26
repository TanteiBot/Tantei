#region LICENSE

// PaperMalKing.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.Services.Background
{
	public sealed class UpdateProvidersManagementService : IHostedService
	{
		private readonly ILogger<UpdateProvidersManagementService> _logger;
		private readonly IServiceProvider _serviceProvider;
		private readonly UpdateProvidersConfigurationService _updateProvidersConfigurationService;


		public UpdateProvidersManagementService(ILogger<UpdateProvidersManagementService> logger, IServiceProvider serviceProvider,
												UpdateProvidersConfigurationService updateProvidersConfigurationService)
		{
			this._logger = logger;

			this._logger.LogTrace("Building {@UpdateProvidersManagementService}", typeof(UpdateProvidersManagementService));
			this._serviceProvider = serviceProvider;
			this._updateProvidersConfigurationService = updateProvidersConfigurationService;
			this._logger.LogTrace("Built {@UpdateProvidersManagementService}", typeof(UpdateProvidersManagementService));
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			this._logger.LogInformation("Starting to wait for shutdown for cancelling update providers checking for updates");
			return Task.CompletedTask;
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			this._logger.LogInformation("Stopping update providers");
			var tasks = new Task[this._updateProvidersConfigurationService.Providers.Count];
			var providers = this._updateProvidersConfigurationService.Providers.Values.ToArray();
			for (var i = 0; i < providers.Length; i++)
			{
				var provider = providers[i];
				tasks[i] = provider.TriggerStoppingAsync();
			}

			await Task.WhenAll(tasks).ConfigureAwait(false);
		}
	}
}