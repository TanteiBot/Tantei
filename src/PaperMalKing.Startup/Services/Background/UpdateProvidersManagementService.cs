// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.Startup.Services.Background;

internal sealed class UpdateProvidersManagementService(ILogger<UpdateProvidersManagementService> _logger, UpdateProvidersConfigurationService _updateProvidersConfigurationService)
	: IHostedService
{
	public Task StartAsync(CancellationToken cancellationToken)
	{
		_logger.StartingToWaitForShutdown();
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.StoppingUpdateProviders();
		var tasks = new Task[_updateProvidersConfigurationService.Providers.Count];
		var providers = _updateProvidersConfigurationService.Providers.Values.ToArray();
		for (var i = 0; i < providers.Length; i++)
		{
			var provider = providers[i];
			tasks[i] = provider.TriggerStoppingAsync();
		}

		return Task.WhenAll(tasks);
	}
}