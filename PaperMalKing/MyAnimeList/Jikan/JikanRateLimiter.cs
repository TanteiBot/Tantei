using System.Threading.Tasks;
using DSharpPlus;
using PaperMalKing.Data;
using PaperMalKing.Services;
using PaperMalKing.Utilities;

namespace PaperMalKing.MyAnimeList.Jikan
{
	public sealed class JikanRateLimiter : RateLimiter
	{
		/// <inheritdoc />
		public JikanRateLimiter(RateLimit rateLimit, ClockService clock, LogDelegate log) : base(rateLimit, clock,
			"JknRateLimiter", log)
		{ }

		public async Task PopulateTokenAsync(RateLimiterToken token)
		{
			await this.SemaphoreSlim.WaitAsync();
			try
			{
				this.Tokens.Enqueue(token);
				this.Log(LogLevel.Debug, this.RateLimiterName, "Populating token", this.Clock.Now);
			}
			finally
			{
				this.SemaphoreSlim.Release();
			}
		}
	}
}