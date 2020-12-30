using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PaperMalKing.Common.RateLimiter
{
	public sealed class LockFreeRateLimiter<T> : IRateLimiter<T>
	{
		private readonly string _serviceName;
		public RateLimit RateLimit { get; }
		private ILogger<IRateLimiter<T>> Logger { get; }
		private long _lastUpdateTime;

		private long _availablePermits;

		private readonly long _delayBetweenRefills;

		private SpinWait _spinner = new();

		internal LockFreeRateLimiter(RateLimit rateLimit, ILogger<IRateLimiter<T>>? logger)
		{
			this._serviceName = $"{typeof(RateLimiter<T>).Name}<{typeof(T).Name}>";
			this.RateLimit = rateLimit;

			this.Logger = logger ?? NullLogger<RateLimiter<T>>.Instance;
			this._lastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			this._availablePermits = rateLimit.AmountOfRequests;
			this._delayBetweenRefills = rateLimit.PeriodInMilliseconds;
		}

		public async Task TickAsync()
		{
			while (true)
			{
				var lastUpdateTime = Interlocked.Read(ref this._lastUpdateTime);
				var availablePermits = Interlocked.Read(ref this._availablePermits);
				var nextRefillDateTime = lastUpdateTime + this._delayBetweenRefills;
				var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
				var arePermitsAvailable = availablePermits > 0;
				var isTooEarlyToRefill = now < nextRefillDateTime;
				switch (isTooEarlyToRefill)
				{
					case true when !arePermitsAvailable:
					{
						var delay = nextRefillDateTime - now;
						var delayInMs = Convert.ToInt32(delay);
						this.Logger.LogDebug($"[{this._serviceName}] Waiting {delayInMs.ToString()}ms.");
						await Task.Delay(delayInMs);
						break;
					}
					// && arePermitsAvailable
					case true:
					{
						this.Logger.LogDebug($"[{this._serviceName}] Passing");
						Interlocked.Decrement(ref this._availablePermits);
						return;
					}
				}

				if (Interlocked.CompareExchange(ref this._lastUpdateTime, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), lastUpdateTime) ==
					lastUpdateTime)
				{
					this.Logger.LogTrace($"[{this._serviceName}] Updating {nameof(this._availablePermits)}");
					Interlocked.Exchange(ref this._availablePermits, this.RateLimit.AmountOfRequests - 1);
					return;
				}
				else
				{
					this.Logger.LogTrace($"[{this._serviceName}] Couldn't update {nameof(this._lastUpdateTime)}. Spinning.");
					this._spinner.SpinOnce();
				}
			}
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"[{this._serviceName}] with rate limits {this.RateLimit}";
		}
	}
}