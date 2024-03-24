// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

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
		this.Timer = new(_ => this.TimerCallback(), state: null, Timeout.Infinite, Timeout.Infinite);
	}

	public abstract string Name { get; }

	public abstract event UpdateFoundEvent? UpdateFoundEvent;

	[SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "We stored it there in our class")]
	public async Task TriggerStoppingAsync()
	{
		await (this._cts?.CancelAsync() ?? Task.CompletedTask);
		this.Logger.StopUpdateProvider(this.Name);
		await this._updateCheckingRunningTask;
	}

	public DateTimeOffset? DateTimeOfNextUpdate { get; private set; }

	public bool IsUpdateInProgress { get; private set; }

	[SuppressMessage("Major Bug", """S3168:"async" methods should not return "void" """, Justification = "We can't use non-async voids in timer delegate")]
	[SuppressMessage("Usage", "VSTHRD100:Avoid async void methods", Justification = "We can't use non-async voids in timer delegate")]
	[SuppressMessage("AsyncUsage", "AsyncFixer03:Fire-and-forget async-void methods or delegates", Justification = "We can't use non-async voids in timer delegate")]
	private async void TimerCallback()
	{
		this.IsUpdateInProgress = true;
		this.DateTimeOfNextUpdate = null;
		using var cts = new CancellationTokenSource();
		this._cts = cts;
		try
		{
			this.Logger.StartCheckingForUpdates(this.Name);
			this._updateCheckingRunningTask = this.CheckForUpdatesAsync(this._cts.Token);
			await this._updateCheckingRunningTask;
		}
		catch (TaskCanceledException) when (cts.IsCancellationRequested)
		{
			// Ignore
			// We were cancelled
		}
		#pragma warning disable CA1031
		// Modify 'TimerCallback' to catch a more specific allowed exception type, or rethrow the exception
		catch (Exception e)
			#pragma warning restore CA1031
		{
			this.Logger.ErrorOnUpdateCheck(e, this.Name);
		}
		finally
		{
			this._cts = null;
			this.RestartTimer(this.DelayBetweenTimerFires);
			this.Logger.EndCheckingForUpdates(this.Name, this.DelayBetweenTimerFires);
			this.IsUpdateInProgress = false;
			this.DateTimeOfNextUpdate = TimeProvider.System.GetUtcNow() + this.DelayBetweenTimerFires;
		}
	}

	public void RestartTimer(TimeSpan delay)
	{
		this.Timer.Change(delay, Timeout.InfiniteTimeSpan);
	}

	protected abstract Task CheckForUpdatesAsync(CancellationToken cancellationToken);
}