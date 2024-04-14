// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.Startup.Services.Background;

internal sealed class UpdateProvidersManagementService : IHostedService
{
	private readonly ILogger<UpdateProvidersManagementService> _logger;
	private readonly UpdateProvidersConfigurationService _updateProvidersConfigurationService;

	public UpdateProvidersManagementService(ILogger<UpdateProvidersManagementService> logger, UpdateProvidersConfigurationService updateProvidersConfigurationService)
	{
		this._logger = logger;

		this._updateProvidersConfigurationService = updateProvidersConfigurationService;
	}

	public Task StartAsync(CancellationToken cancellationToken)
	{
		this._logger.StartingToWaitForShutdown();
		return Task.CompletedTask;
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		this._logger.StoppingUpdateProviders();
		var tasks = new Task[this._updateProvidersConfigurationService.Providers.Count];
		var providers = this._updateProvidersConfigurationService.Providers.Values.ToArray();
		for (var i = 0; i < providers.Length; i++)
		{
			var provider = providers[i];
			tasks[i] = provider.TriggerStoppingAsync();
		}

		return Task.WhenAll(tasks);
	}
}