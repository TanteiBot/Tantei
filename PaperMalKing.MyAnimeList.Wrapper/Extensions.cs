// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

namespace PaperMalKing.MyAnimeList.Wrapper;

internal static class Extensions
{
	internal static bool Has(this MangaFieldsToRequest fields, MangaFieldsToRequest field)
	{
		return (fields & field) != 0;
	}
	
	internal static bool Has(this AnimeFieldsToRequest fields, AnimeFieldsToRequest field)
	{
		return (fields & field) != 0;
	}
}