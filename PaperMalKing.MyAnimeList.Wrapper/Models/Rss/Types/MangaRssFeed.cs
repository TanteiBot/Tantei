using PaperMalKing.Common.Enums;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.Rss.Types
{
	internal readonly struct MangaRssFeed : IRssFeedType
	{
		public string Url => Constants.RSS_MANGA_URL;

		public ListEntryType Type => ListEntryType.Manga;
	}
}