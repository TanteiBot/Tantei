namespace PaperMalKing.Shikimori.Wrapper.Models.List
{
	internal readonly struct AnimeListType : IListType
	{
		/// <inheritdoc />
		public string ListType => "anime_rates";
	}

	internal readonly struct MangaListType : IListType
	{
		/// <inheritdoc />
		public string ListType => "manga_rates";
	}

	internal interface IListType
	{
		string ListType { get; }
	}
}