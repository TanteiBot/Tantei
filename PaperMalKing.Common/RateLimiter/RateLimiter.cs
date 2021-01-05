using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PaperMalKing.Common.RateLimiter
{
	public sealed class RateLimiter<T> : IRateLimiter<T>, IDisposable
	{
		private readonly string _serviceName;
		public RateLimit RateLimit { get; }
		private ILogger<IRateLimiter<T>> Logger { get; }
		private long _lastUpdateTime;

		private long _availablePermits;
		private SemaphoreSlim? _semaphoreSlim;

		internal RateLimiter(RateLimit rateLimit, ILogger<IRateLimiter<T>>? logger)
		{
			this._serviceName = $"{typeof(RateLimiter<T>).Name}<{typeof(T).Name}>";
			this.RateLimit = rateLimit;

			this.Logger = logger ?? NullLogger<RateLimiter<T>>.Instance;
			this._lastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			this._availablePermits = rateLimit.AmountOfRequests;
			this._semaphoreSlim = new (1, 1);
		}

		public async Task TickAsync()
		{
			if (this._semaphoreSlim == null)
				return;
			await this._semaphoreSlim.WaitAsync();
			try
			{
				var nextRefillDateTime = this._lastUpdateTime + this.RateLimit.PeriodInMilliseconds;
				var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
				var arePermitsAvailable = this._availablePermits > 0;
				var isTooEarlyToRefill = now < nextRefillDateTime;
				if (isTooEarlyToRefill && !arePermitsAvailable)
				{
					var delay = nextRefillDateTime - now;
					var delayInMs = Convert.ToInt32(delay);
					this.Logger.LogDebug(
						$"[{this._serviceName}] Waiting {delayInMs.ToString()}ms.");
					await Task.Delay(delayInMs);
				}
				else if (isTooEarlyToRefill) // && arePermitsAvailable
				{
					this.Logger.LogInformation("[{ServiceName}] Passing", this._serviceName);
					this._availablePermits--;
					return;
				}

				this._lastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
				this._availablePermits = this.RateLimit.AmountOfRequests - 1;
			}
			finally
			{
				this._semaphoreSlim?.Release();
			}
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"[{this._serviceName}] with rate limits {this.RateLimit}";
		}

		/// <inheritdoc />
		public void Dispose()
		{
			var semaphore = this._semaphoreSlim;
			this._semaphoreSlim = null;
			semaphore?.Dispose();
		}
	}
}