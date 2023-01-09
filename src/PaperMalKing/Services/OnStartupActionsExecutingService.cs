// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PaperMalKing.Database;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.Services;

internal sealed class OnStartupActionsExecutingService : IHostedService
{
	private readonly IServiceProvider _serviceProvider;

	public OnStartupActionsExecutingService(IServiceProvider serviceProvider)
	{
		this._serviceProvider = serviceProvider;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		using var scope = this._serviceProvider.CreateScope();

		scope.ServiceProvider.GetRequiredService<ICommandsService>();
		_ = scope.ServiceProvider.GetRequiredService<UpdatePublishingService>();
		var services = scope.ServiceProvider.GetServices<IExecuteOnStartupService>();
		foreach (var service in services)
		{
			if (cancellationToken.IsCancellationRequested)
				return;

			await service.ExecuteAsync(cancellationToken).ConfigureAwait(false);
		}
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}