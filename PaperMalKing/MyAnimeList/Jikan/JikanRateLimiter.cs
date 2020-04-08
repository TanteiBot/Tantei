using System.Threading.Tasks;
using PaperMalKing.Data;
using PaperMalKing.Services;

namespace PaperMalKing.MyAnimeList.Jikan
{
	public class JikanRateLimiter : RateLimiter
	{
		/// <inheritdoc />
		public JikanRateLimiter(RateLimit rateLimit, ClockService clock) : base(rateLimit, clock)
		{ }

		public async Task PopulateToken(RateLimiterToken token)
		{
			await this.SemaphoreSlim.WaitAsync();
			try
			{
				this.Tokens.Enqueue(token);
			}
			finally
			{
				this.SemaphoreSlim.Release();
			}
		}
	}
}