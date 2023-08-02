// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Diagnostics.CodeAnalysis;

namespace PaperMalKing.Database.Models.MyAnimeList;

[SuppressMessage("Design", "CA1008:Enums should have zero value")]
public enum MalFavoriteType : byte
{
	Anime = 1,
	Manga = 2,
	Character = 3,
	Person = 4,
	Company = 5,
}