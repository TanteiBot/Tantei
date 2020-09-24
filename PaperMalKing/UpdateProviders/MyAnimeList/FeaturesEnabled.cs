using System;

namespace PaperMalKing.UpdateProviders.MyAnimeList
{
	[Flags]
	public enum FeaturesEnabled : long
	{
		AnimeUpdates = 1,

		MangaUpdates = 2,

		AllListsUpdates = 3,

		Favorites = 4,

		TagsSupport = 8
	}
}