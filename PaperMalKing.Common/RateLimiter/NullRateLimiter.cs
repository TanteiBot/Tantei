using System.Threading.Tasks;

namespace PaperMalKing.Common.RateLimiter
{
	public sealed class NullRateLimiter<T> : IRateLimiter<T>
	{
		public static readonly NullRateLimiter<T> Instance = new(new(1, 1));

		internal NullRateLimiter(RateLimit rateLimit)
		{
			this.RateLimit = rateLimit;
		}

		/// <inheritdoc />
		public RateLimit RateLimit { get; }

		public Task TickAsync() => Task.CompletedTask;
	}
}