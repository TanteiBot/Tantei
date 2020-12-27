using PaperMalKing.Common.Enums;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.List.Types
{
	internal interface IListType<T> where T : class, IListEntry
	{
		ListEntryType ListEntryType { get; }
		string LatestUpdatesUrl(string username);
	}
}