using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Options;

namespace PaperMalKing.Services.Background
{
	public sealed class TimerService : IHostedService, IAsyncDisposable
	{
		private readonly ILogger<TimerService> _logger;
		private readonly UpdateProvidersManagementService _updateProvidersManagementService;
		private readonly IOptionsMonitor<TimerOptions> _options;
		private readonly Timer _timer;

		public TimerService(ILogger<TimerService> logger, IOptionsMonitor<TimerOptions> optionsMonitor,
							UpdateProvidersManagementService updateProvidersManagementService)
		{
			this._logger = logger;
			this._updateProvidersManagementService = updateProvidersManagementService;
			this._options = optionsMonitor;
			this._timer = new Timer(TimerCallback, null, TimeSpan.FromMilliseconds(-1), TimeSpan.FromMilliseconds(-1));
		}

		private void TimerCallback(object? state = null)
		{
			this._logger.LogInformation("Started checking for updates");
			var cts = new CancellationTokenSource(this._options.CurrentValue.ResetTimerAfterMilliseconds);
			try
			{
				var tasks = this._updateProvidersManagementService.StartUpdateChecking();
				Task.WaitAll(tasks, cts.Token);
				this._logger.LogInformation("Stopped checking for updates");
			}
			finally
			{
				this._timer.Change(TimeSpan.FromMilliseconds(this._options.CurrentValue.MillisecondsDelay), TimeSpan.FromMilliseconds(-1));
			}
		}

		/// <inheritdoc />
		public Task StartAsync(CancellationToken cancellationToken)
		{
			this._timer.Change(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(-1));
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public Task StopAsync(CancellationToken cancellationToken)
		{
			this._timer.Change(Timeout.Infinite, Timeout.Infinite);
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public ValueTask DisposeAsync()
		{
			return this._timer.DisposeAsync();
		}
	}
}