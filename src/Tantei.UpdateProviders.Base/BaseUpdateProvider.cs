// Tantei.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

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