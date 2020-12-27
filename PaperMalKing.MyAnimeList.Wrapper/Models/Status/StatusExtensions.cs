namespace PaperMalKing.MyAnimeList.Wrapper.Models.Status
{
	internal static class StatusExtensions
	{
		internal static GenericStatus ToGeneric(this AnimeStatus @this) => (GenericStatus) @this;

		internal static GenericStatus ToGeneric(this MangaStatus @this) => (GenericStatus) @this;

		internal static AnimeStatus ToAnimeStatus(this GenericStatus @this) => (AnimeStatus) @this;

		internal static MangaStatus ToMangaStatus(this GenericStatus @this) => (MangaStatus) @this;
	}
}