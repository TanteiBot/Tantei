using System;
using PaperMalKing.MyAnimeList.Wrapper.Models.Progress;

namespace PaperMalKing.MyAnimeList.Wrapper.Parsers
{
	internal static class ProgressParser
	{
		internal static GenericProgress Parse(string value)
		{
			value = value.Trim();
			if (value == "" || value == "?")
				return GenericProgress.Reprogressing;
			value = value.Replace("-", "").Replace(" ", "");
			if (TryParse<GenericProgress>(value, out var genericResult))
				return genericResult;
			if (TryParse<AnimeProgress>(value, out var animeResult))
				return animeResult.ToGeneric();
			if (TryParse<MangaProgress>(value, out var mangaResult))
				return mangaResult.ToGeneric();
			return GenericProgress.Unknown;
		}

		private static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct, Enum =>
			Enum.TryParse(value, true, out result);
	}
}