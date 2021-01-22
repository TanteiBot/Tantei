#region LICENSE
// PaperMalKing.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

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