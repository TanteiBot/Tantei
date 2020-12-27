using PaperMalKing.Common.Enums;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.Rss.Types
{
	internal readonly struct AnimeRssFeed : IRssFeedType
	{
		public string Url => Constants.RSS_ANIME_URL;

		public ListEntryType Type => ListEntryType.Anime;
	}
}