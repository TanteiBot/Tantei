using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PaperMalKing.Data.RateLimiter
{
	public class RateLimiter : IRateLimiter
	{
		public RateLimit RateLimit { get; protected set; }
		protected ILogger<RateLimiter> Logger { get; }
		private long _lastUpdateTime;

		protected int AvailablePermits;
		protected readonly SemaphoreSlim SemaphoreSlim;

		public RateLimiter(RateLimit rateLimit, string appName, ILogger<RateLimiter>? logger = null)
		{
			this.RateLimit = rateLimit;

			this.Logger = logger ?? NullLogger<RateLimiter>.Instance;
			this._lastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			this.AvailablePermits = rateLimit.AmountOfRequests;
			this.SemaphoreSlim = new SemaphoreSlim(1, 1);
		}

		public async Task TickAsync()
		{
			await this.SemaphoreSlim.WaitAsync();
			try
			{
				var nextRefillDateTime = this._lastUpdateTime + this.RateLimit.TimeConstraint;
				var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
				var arePermitsAvailable = this.AvailablePermits == 0;
				var isTooEarlyToRefill = now < nextRefillDateTime;
				if (isTooEarlyToRefill && !arePermitsAvailable)
				{
					var delay = nextRefillDateTime - now;
					var delayInMs = Convert.ToInt32(delay);
					this.Logger.LogInformation($"Waiting {delayInMs.ToString()}ms before getting next token.");
					await Task.Delay(delayInMs);
				}
				else if (isTooEarlyToRefill) // && TokensAreAvailable
				{
					this.AvailablePermits--;
					return;
				}

				this._lastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
				this.AvailablePermits = this.RateLimit.AmountOfRequests - 1;
			}
			finally
			{
				this.SemaphoreSlim.Release();
			}
		}
	}
}