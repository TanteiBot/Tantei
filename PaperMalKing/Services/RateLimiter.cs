using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PaperMalKing.Data;

namespace PaperMalKing.Services
{
	public class RateLimiter
	{
		public readonly RateLimit RateLimit;
		private readonly ClockService _clock;

		private DateTime _lastUpdateTime;

		protected readonly FixedSizeQueue<RateLimiterToken> Tokens;

		protected readonly SemaphoreSlim SemaphoreSlim;

		public RateLimiter(RateLimit rateLimit, ClockService clock)
		{
			this.RateLimit = rateLimit;
			this._clock = clock;
			this.SemaphoreSlim = new SemaphoreSlim(1, 1);
			this.Tokens = new FixedSizeQueue<RateLimiterToken>(this.RateLimit.AmountOfRequests);
		}

		public async Task<RateLimiterToken> GetTokenAsync()
		{
			await this.SemaphoreSlim.WaitAsync();
			try
			{
				var nextRefillDateTime = this._lastUpdateTime.Add(this.RateLimit.TimeConstraint);
				var now = this._clock.UtcNow;
				var areTokensAvailable = this.Tokens.Any();
				var isTooEarlyToRefill = DateTime.Compare(now, nextRefillDateTime) < 0;
				if (isTooEarlyToRefill && !areTokensAvailable)
				{
					var delay = (nextRefillDateTime - now).Duration();
					await Task.Delay(delay);
				}
				else if (isTooEarlyToRefill) // && TokensAreAvailable
				{
					return this.Tokens.Dequeue();
				}
				else // Removing old tokens to generate new
				{
					this.Tokens.Clear();
				}

				this._lastUpdateTime = this._clock.UtcNow;
				for (int i = 0; i < this.RateLimit.AmountOfRequests - 1; i++)
					this.Tokens.Enqueue(new RateLimiterToken(this._clock.UtcNow));
				return new RateLimiterToken(this._clock.UtcNow);
			}
			finally
			{
				this.SemaphoreSlim.Release();
			}
		}
	}
}