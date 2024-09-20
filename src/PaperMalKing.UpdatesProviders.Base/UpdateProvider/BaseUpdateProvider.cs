// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;

namespace PaperMalKing.UpdatesProviders.Base.UpdateProvider;

public abstract class BaseUpdateProvider : BackgroundService, IUpdateProvider
{
	public abstract string Name { get; }

	public abstract event AsyncEventHandler<UpdateFoundEventArgs>? UpdateFoundEvent;

	protected abstract TimeSpan DelayBetweenTimerFires { get; }

	protected abstract Task CheckForUpdatesAsync(CancellationToken cancellationToken);

	public DateTimeOffset? DateTimeOfNextUpdate { get; private set; }

	public bool IsUpdateInProgress { get; private set; }

	protected ILogger<BaseUpdateProvider> Logger { get; }

	private CancellationTokenSource _restartTokenSource;

	protected BaseUpdateProvider(ILogger<BaseUpdateProvider> logger)
	{
		this._restartTokenSource = new();
		this.Logger = logger;
	}

	public override Task StopAsync(CancellationToken cancellationToken)
	{
		this.Logger.StopUpdateProvider(this.Name);
		return base.StopAsync(cancellationToken);
	}

	public void StartOrRestartAfter(TimeSpan delay)
	{
		this._restartTokenSource.CancelAfter(delay);
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!this._restartTokenSource.IsCancellationRequested)
		{
			await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
		}

		var rts = this._restartTokenSource;

		this._restartTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
		rts.Dispose();

		while (!stoppingToken.IsCancellationRequested)
		{
			if (this._restartTokenSource.IsCancellationRequested)
			{
				rts = this._restartTokenSource;

				this._restartTokenSource = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
				rts.Dispose();
			}

			this.IsUpdateInProgress = true;
			this.DateTimeOfNextUpdate = null;
			TimeSpan delayBetweenTimerFires;
			var scope = this.Logger.BeginScope("Update checking");
			try
			{
				this.Logger.StartCheckingForUpdates(this.Name);
				await this.CheckForUpdatesAsync(stoppingToken);
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				// Ignore
				// We were cancelled
			}
#pragma warning disable CA1031
			// Modify 'ExecuteAsync' to catch a more specific allowed exception type, or rethrow the exception
			catch (Exception e)
#pragma warning restore CA1031
			{
				this.Logger.ErrorOnUpdateCheck(e, this.Name);
			}
			finally
			{
				scope?.Dispose();
				delayBetweenTimerFires = this.DelayBetweenTimerFires;
				this.Logger.EndCheckingForUpdates(this.Name, delayBetweenTimerFires);
				this.IsUpdateInProgress = false;
				this.DateTimeOfNextUpdate = TimeProvider.System.GetUtcNow() + delayBetweenTimerFires;
			}

			try
			{
				await Task.Delay(delayBetweenTimerFires, this._restartTokenSource.Token);
			}
#pragma warning disable CA1031, ERP022
			// Modify 'ExecuteAsync' to catch a more specific allowed exception type, or rethrow the exception
			catch
			{
				// Ignore
				// Unobserved exception in a generic exception handler
			}
#pragma warning restore ERP022, CA1031
		}
	}

	[SuppressMessage("Major Code Smell", "S3971:\"GC.SuppressFinalize\" should not be called", Justification = "We call it in dispose method")]
	public override void Dispose()
	{
		GC.SuppressFinalize(this);
		this._restartTokenSource.Dispose();
		base.Dispose();
	}
}