using PaperMalKing.Common.Options;

namespace PaperMalKing.AniList.UpdateProvider
{
    internal sealed class AniListOptions : ITimerOptions<AniListUpdateProvider>
    {
        public int DelayBetweenChecksInMilliseconds { get; init; }

        public const string AniList = Constants.NAME;
    }
}