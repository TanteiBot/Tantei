using PaperMalKing.Common.Options;

namespace PaperMalKing.Shikimori.UpdateProvider
{
	internal sealed class ShikiOptions : ITimerOptions<ShikiUpdateProvider>
	{
		public const string Shikimori = Constants.NAME;

		public string ShikimoriAppName { get; init; } = null!;
		
		/// <inheritdoc />
		public int DelayBetweenChecksInMilliseconds { get; init; }
	}
}