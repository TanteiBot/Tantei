using PaperMalKing.Common.Enums;

namespace PaperMalKing.Shikimori.Wrapper.Models.List
{
	internal readonly struct AnimeListType : IListType
	{
		/// <inheritdoc />
		public string ListType => "anime_rates";

		/// <inheritdoc />
		public ListEntryType ListEntryType => ListEntryType.Anime;
	}

	internal readonly struct MangaListType : IListType
	{
		/// <inheritdoc />
		public string ListType => "manga_rates";

		/// <inheritdoc />
		public ListEntryType ListEntryType => ListEntryType.Manga;
	}

	internal interface IListType
	{
		string ListType { get; }
		ListEntryType ListEntryType { get; }
	}
}