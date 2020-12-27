using System;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.UpdatesProviders.Base.UpdateProvider
{
	public abstract class BaseUpdateProvider : IUpdateProvider
	{
		private readonly CancellationTokenSource _cts;

		protected readonly ILogger<BaseUpdateProvider> Logger;

		protected readonly Timer Timer;

		protected readonly TimeSpan _delayBetweenTimerFires;

		private Task _updateCheckingRunningTask = null!;

		protected BaseUpdateProvider(ILogger<BaseUpdateProvider> logger, TimeSpan delayBetweenTimerFires)
		{
			this._cts = new();
			this.Logger = logger;
			this._delayBetweenTimerFires = delayBetweenTimerFires;
			this.Timer = new(_ => this.TimerCallback(null), null, Timeout.Infinite, Timeout.Infinite);

		}

		/// <inheritdoc />
		public abstract string Name { get; }

		/// <inheritdoc />
		public abstract event UpdateFoundEventHandler UpdateFoundEvent;

		/// <inheritdoc />
		public Task TriggerStoppingAsync()
		{
			this._cts.Cancel();
			return this._updateCheckingRunningTask;
		}

		private async void TimerCallback(object? state)
		{
			try
			{
				this.Logger.LogInformation($"Starting checking for updates in {this.Name} updates provider");
				this._updateCheckingRunningTask = this.CheckForUpdatesAsync(this._cts.Token);
				await this._updateCheckingRunningTask;
			}
			catch (Exception e)
			{
				this.Logger.LogError(e, $"Exception occured while checking for updates in {this.Name} updates provider");
			}
			finally
			{
				this.RestartTimer(this._delayBetweenTimerFires);
				this.Logger.LogInformation($"Ended checking for updates in {this.Name} updates provider. Next planned update check is in {this._delayBetweenTimerFires.Humanize()}.");
			}
		}

		public void RestartTimer(TimeSpan delay)
		{
			this.Timer.Change(delay, Timeout.InfiniteTimeSpan);
		}

		protected abstract Task CheckForUpdatesAsync(CancellationToken cancellationToken);
	}
}