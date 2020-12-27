using PaperMalKing.Common.Enums;
using static PaperMalKing.MyAnimeList.Wrapper.Constants;


namespace PaperMalKing.MyAnimeList.Wrapper.Models.List.Types
{
	internal readonly struct MangaListType : IListType<MangaListEntry>
	{
		public ListEntryType ListEntryType => ListEntryType.Manga;

		public string LatestUpdatesUrl(string username) => MANGA_LIST_URL + username + LATEST_LIST_UPDATES;
	}
}