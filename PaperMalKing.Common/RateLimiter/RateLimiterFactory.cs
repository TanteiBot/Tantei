using Microsoft.Extensions.Logging;

namespace PaperMalKing.Common.RateLimiter
{
	public static class RateLimiterFactory
	{
		public static IRateLimiter<T> Create<T>(RateLimit rateLimit, ILogger<IRateLimiter<T>>? logger = null)
		{
			if (rateLimit.AmountOfRequests == 0 || rateLimit.PeriodInMilliseconds == 0)
				return new NullRateLimiter<T>(rateLimit);

			return new RateLimiter<T>(rateLimit, logger);
		}
	}
}