// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace Tantei.Core.Models.AniList.Users;

public enum AniListFavoriteType : byte
{
	None = 0,
	Anime = 1,
	Manga = 2,
	Character = 3,
	Staff = 4,
	Studio = 5
}