// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace Tantei.Core.Models.AniList.Users;

public sealed record AniListFavorite(ulong Id, AniListFavoriteType Type, ulong UserId, AniListUser User);