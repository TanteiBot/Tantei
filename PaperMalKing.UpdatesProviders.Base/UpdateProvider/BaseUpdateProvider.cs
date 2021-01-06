using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.UpdatesProviders.Base.UpdateProvider
{
	public abstract class BaseUpdateProvider : IUpdateProvider
	{
		private readonly CancellationTokenSource _cts;

		protected readonly ILogger<BaseUpdateProvider> Logger;

		protected readonly Timer Timer;

		protected readonly TimeSpan DelayBetweenTimerFires;

		private Task _updateCheckingRunningTask = null!;

		protected BaseUpdateProvider(ILogger<BaseUpdateProvider> logger, TimeSpan delayBetweenTimerFires)
		{
			this._cts = new();
			this.Logger = logger;
			this.DelayBetweenTimerFires = delayBetweenTimerFires;
			this.Timer = new(_ => this.TimerCallback(), null, Timeout.Infinite, Timeout.Infinite);

		}

		/// <inheritdoc />
		public abstract string Name { get; }

		/// <inheritdoc />
		public abstract event UpdateFoundEventHandler? UpdateFoundEvent;

		/// <inheritdoc />
		public Task TriggerStoppingAsync()
		{
			this._cts.Cancel();
			this.Logger.LogInformation("Stopping {Name} update provider", this.Name);
			return this._updateCheckingRunningTask;
		}

		private async void TimerCallback()
		{
			try
			{
				this.Logger.LogInformation("Starting checking for updates in {@Name} updates provider", this.Name);
				this._updateCheckingRunningTask = this.CheckForUpdatesAsync(this._cts.Token);
				await this._updateCheckingRunningTask;
			}
			catch (Exception e)
			{
				this.Logger.LogError(e, "Exception occured while checking for updates in {@Name} updates provider", this.Name);
			}
			finally
			{
				this.RestartTimer(this.DelayBetweenTimerFires);
				this.Logger.LogInformation("Ended checking for updates in {Name} updates provider. Next planned update check is in {@DelayBetweenTimerFires}.", this.Name, this.DelayBetweenTimerFires);
			}
		}

		public void RestartTimer(TimeSpan delay)
		{
			this.Timer.Change(delay, Timeout.InfiniteTimeSpan);
		}

		protected abstract Task CheckForUpdatesAsync(CancellationToken cancellationToken);
	}
}