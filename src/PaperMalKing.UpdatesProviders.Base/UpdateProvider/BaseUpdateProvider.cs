// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.UpdatesProviders.Base.UpdateProvider;

public abstract class BaseUpdateProvider : IUpdateProvider
{
	private CancellationTokenSource? _cts;

	protected ILogger<BaseUpdateProvider> Logger { get; }

	protected Timer Timer { get; }

	protected TimeSpan DelayBetweenTimerFires { get; }

	private Task _updateCheckingRunningTask = Task.CompletedTask;

	protected BaseUpdateProvider(ILogger<BaseUpdateProvider> logger, TimeSpan delayBetweenTimerFires)
	{
		this.Logger = logger;
		this.DelayBetweenTimerFires = delayBetweenTimerFires;
		this.Timer = new(_ => this.TimerCallback(), null, Timeout.Infinite, Timeout.Infinite);
	}

	public abstract string Name { get; }

	public abstract event UpdateFoundEvent? UpdateFoundEvent;

	[SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks")]
	public Task TriggerStoppingAsync()
	{
		this._cts?.Cancel();
		this.Logger.LogInformation("Stopping {Name} update provider", this.Name);
		return this._updateCheckingRunningTask;
	}

	[SuppressMessage("Major Bug", "S3168:\"async\" methods should not return \"void\"")]
	[SuppressMessage("", "AsyncFixer03")]
	[SuppressMessage("Usage", "VSTHRD100:Avoid async void methods")]
	private async void TimerCallback()
	{
		using var cts = new CancellationTokenSource();
		this._cts = cts;
		try
		{
			this.Logger.LogInformation("Starting checking for updates in {@Name} updates provider", this.Name);
			this._updateCheckingRunningTask = this.CheckForUpdatesAsync(this._cts.Token);
			await this._updateCheckingRunningTask.ConfigureAwait(false);
		}
		catch (TaskCanceledException) when (cts.IsCancellationRequested)
		{
			// Ignore
			// We were cancelled
		}
#pragma warning disable CA1031
		catch (Exception e)
#pragma warning restore CA1031
		{
			this.Logger.LogError(e, "Exception occured while checking for updates in {@Name} updates provider", this.Name);
		}
		finally
		{
			this._cts = null;
			this.RestartTimer(this.DelayBetweenTimerFires);
			this.Logger.LogInformation(
				"Ended checking for updates in {Name} updates provider. Next planned update check is in {@DelayBetweenTimerFires}.", this.Name,
				this.DelayBetweenTimerFires);
			Environment.Exit(0);
		}
	}

	public void RestartTimer(TimeSpan delay)
	{
		this.Timer.Change(delay, Timeout.InfiniteTimeSpan);
	}

	protected abstract Task CheckForUpdatesAsync(CancellationToken cancellationToken);
}