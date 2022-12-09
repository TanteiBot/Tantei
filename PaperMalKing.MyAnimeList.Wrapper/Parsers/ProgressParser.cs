// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using PaperMalKing.MyAnimeList.Wrapper.Models.Progress;

namespace PaperMalKing.MyAnimeList.Wrapper.Parsers
{
	internal static class ProgressParser
	{
		internal static GenericProgress Parse(string value)
		{
			value = value.Trim();
			if (value.Length == 0 || value == "?")
				return GenericProgress.Reprogressing;
			value = value.Replace("-", "", StringComparison.Ordinal).Replace(" ", "", StringComparison.Ordinal);
			if (TryParse<GenericProgress>(value, out var genericResult))
				return genericResult;
			if (TryParse<AnimeProgress>(value, out var animeResult))
				return animeResult.ToGeneric();
			if (TryParse<MangaProgress>(value, out var mangaResult))
				return mangaResult.ToGeneric();
			return GenericProgress.Unknown;
		}

		private static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : unmanaged, Enum =>
			Enum.TryParse(value, true, out result);
	}
}