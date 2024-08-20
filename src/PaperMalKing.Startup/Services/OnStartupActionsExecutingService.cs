// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.Startup.Services;

internal sealed class OnStartupActionsExecutingService(IServiceScopeFactory _serviceScopeFactory) : IHostedService
{
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		using var scope = _serviceScopeFactory.CreateScope();

		scope.ServiceProvider.GetRequiredService<ICommandsService>();
		_ = scope.ServiceProvider.GetRequiredService<UpdatePublishingService>();
		foreach (var service in scope.ServiceProvider.GetServices<IExecuteOnStartupService>())
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}

			await service.ExecuteAsync(cancellationToken);
		}
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}