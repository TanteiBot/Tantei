namespace PaperMalKing.MyAnimeList.Wrapper.Models.Progress
{
	internal static class ProgressExtensions
	{
		internal static GenericProgress ToGeneric(this MangaProgress @this) => (GenericProgress) @this;

		internal static GenericProgress ToGeneric(this AnimeProgress @this) => (GenericProgress) @this;

		internal static MangaProgress ToMangaProgress(this GenericProgress @this) => (MangaProgress) @this;

		internal static AnimeProgress ToAnimeProgress(this GenericProgress @this) => (AnimeProgress) @this;
	}
}