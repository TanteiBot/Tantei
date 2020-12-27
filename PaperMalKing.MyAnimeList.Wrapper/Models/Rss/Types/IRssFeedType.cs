using PaperMalKing.Common.Enums;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.Rss.Types
{
	internal interface IRssFeedType
	{
		string Url { get; }

		ListEntryType Type { get; }
	}
}