using System.Threading.Tasks;
using DSharpPlus;
using PaperMalKing.Data;
using PaperMalKing.Services;

namespace PaperMalKing.MyAnimeList.Jikan
{
	public sealed class JikanRateLimiter : RateLimiter
	{
		/// <inheritdoc />
		public JikanRateLimiter(RateLimit rateLimit, ClockService clock, LogService logServiceService) : base(rateLimit,
			"Jkn", clock, logServiceService)
		{ }

		public async Task PopulateTokenAsync(RateLimiterToken token)
		{
			await this.SemaphoreSlim.WaitAsync();
			try
			{
				this.Tokens.Enqueue(token);
				this.LogService.Log(LogLevel.Debug, this.RateLimiterName, "Populating token");
			}
			finally
			{
				this.SemaphoreSlim.Release();
			}
		}
	}
}