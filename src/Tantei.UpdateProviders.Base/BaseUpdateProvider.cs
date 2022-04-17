// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tantei.Data.Abstractions;
using Tantei.UpdateProviders.Abstractions;

namespace Tantei.UpdateProviders.Base;

public abstract partial class BaseUpdateProvider<TUser> : IUpdateProvider where TUser : class, Tantei.Core.Models.Users.IUpdateProviderUser
{
	private readonly CancellationTokenSource _cts;
	private readonly TimeSpan _delayBetweenTimers;
	private readonly IServiceProvider _serviceProvider;
	private readonly Timer _timer;
	private Task? _currentTask;
	private bool _isStarted;
	protected ILogger<BaseUpdateProvider<TUser>> Logger { get; }

	public ValueTask DisposeAsync()
	{
		this._cts.Cancel();
		var timerDisposeValueTask = this._timer.DisposeAsync();
		GC.SuppressFinalize(this);
		this._cts.Dispose();
		return timerDisposeValueTask;
	}

	public abstract string Name { get; }

	public event UpdateFoundEvent? UpdateFound;

	[SuppressMessage("Performance", "CA1848", MessageId = "Use the LoggerMessage delegates")]
	public void Start()
	{
		if (this._isStarted)
		{
			this.Logger.LogError(EventIds.TryingToStartAlreadyStartedUpdateProvider, "Something tried to start already started update provider");
			throw new InvalidOperationException("Update provider have already started.");
		}

		this._isStarted = true;
		this._timer.Change(this._delayBetweenTimers, Timeout.InfiniteTimeSpan);
		this.Logger.LogInformation(EventIds.StartingUpdateProvider, "Starting update provider");
	}

	protected BaseUpdateProvider(ILogger<BaseUpdateProvider<TUser>> logger, IServiceProvider serviceProvider, TimeSpan delayBetweenTimers)
	{
		this._serviceProvider = serviceProvider;
		this._delayBetweenTimers = delayBetweenTimers;
		this.Logger = logger;
		this._timer = new(_ => this.TimerCallback(), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
		this._cts = new();
		this._isStarted = false;
	}

	private async void TimerCallback()
	{
		if (this._cts.IsCancellationRequested)
		{
			return;
		}

		try
		{
			LogUpdateCheckStart(this.Logger);
			using var scope = this._serviceProvider.CreateScope();
			var userDbContext = scope.ServiceProvider.GetRequiredService<IUserDbContext<TUser>>();
			this._currentTask = this.CheckForUpdatesAsync(userDbContext, this._cts.Token);
			await this._currentTask.ConfigureAwait(false);
		}
#pragma warning disable CA1031
		catch (Exception ex)
#pragma warning restore CA1031
		{
			LogUpdateCheckError(this.Logger, ex);
		}
		finally
		{
			this._timer.Change(this._delayBetweenTimers, Timeout.InfiniteTimeSpan);
			LogUpdateCheckFinish(this.Logger, this._delayBetweenTimers);
		}
	}

	protected abstract Task CheckForUpdatesAsync(IUserDbContext<TUser> dbContext, CancellationToken cancellationToken);

	[LoggerMessage(EventIds.UpdateCheckStart, LogLevel.Information, "Starting checking for updates")]
	private static partial void LogUpdateCheckStart(ILogger logger);

	[LoggerMessage(EventIds.UpdateCheckError, LogLevel.Error, "Error occured while checking for updates")]
	private static partial void LogUpdateCheckError(ILogger logger, Exception ex);

	[LoggerMessage(EventIds.UpdateCheckFinish, LogLevel.Information, "Ending checking for updates. Next update will be in {delay}")]
	private static partial void LogUpdateCheckFinish(ILogger logger, TimeSpan delay);

	protected virtual Task? OnUpdateFound(UpdateFoundEventArgs args) => this.UpdateFound?.Invoke(args);
}