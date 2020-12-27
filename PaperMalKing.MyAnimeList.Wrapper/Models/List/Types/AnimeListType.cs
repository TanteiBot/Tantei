using PaperMalKing.Common.Enums;
using static PaperMalKing.MyAnimeList.Wrapper.Constants;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.List.Types
{
	internal readonly struct AnimeListType : IListType<AnimeListEntry>
	{
		public string LatestUpdatesUrl(string username) => ANIME_LIST_URL + username + LATEST_LIST_UPDATES;

		public ListEntryType ListEntryType => ListEntryType.Anime;
	}
}