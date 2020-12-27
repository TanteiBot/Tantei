using PaperMalKing.Common.Options;
using PaperMalKing.MyAnimeList.Wrapper;

namespace PaperMalKing.UpdatesProviders.MyAnimeList
{
	internal sealed class MalOptions : IRateLimitOptions<MyAnimeListClient>, ITimerOptions<MalUpdateProvider>
	{
		public const string MyAnimeList = Constants.Name;

		/// <inheritdoc />
		public int AmountOfRequests { get; init; }

		/// <inheritdoc />
		public int PeriodInMilliseconds { get; init; }

		/// <inheritdoc />
		public int DelayBetweenChecksInMilliseconds { get; init; }
	}
}