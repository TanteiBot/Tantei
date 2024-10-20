﻿// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using PaperMalKing.Database.Models.AniList;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.Database.Models.Shikimori;

namespace PaperMalKing.Database;

public static class DbExtensions
{
	public static AniListUserFeatures GetDefault(this AniListUserFeatures _) => AniListUserFeatures.AnimeList | AniListUserFeatures.MangaList |
																				AniListUserFeatures.Favourites | AniListUserFeatures.Mention |
																				AniListUserFeatures.Website | AniListUserFeatures.MediaFormat |
																				AniListUserFeatures.MediaStatus;

	public static MalUserFeatures GetDefault(this MalUserFeatures _) => MalUserFeatures.AnimeList | MalUserFeatures.MangaList |
																		MalUserFeatures.Favorites | MalUserFeatures.Mention |
																		MalUserFeatures.Website | MalUserFeatures.MediaFormat |
																		MalUserFeatures.MediaStatus;

	public static ShikiUserFeatures GetDefault(this ShikiUserFeatures _) => ShikiUserFeatures.AnimeList | ShikiUserFeatures.MangaList |
																			ShikiUserFeatures.Favourites | ShikiUserFeatures.Mention |
																			ShikiUserFeatures.Website | ShikiUserFeatures.MediaFormat |
																			ShikiUserFeatures.MediaStatus | ShikiUserFeatures.Achievements;
}