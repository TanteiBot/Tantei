// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace Tantei.Core.Models.MyAnimeList.Users;

public sealed record MalUserFavorite(ulong UserId, MalUser User, ulong Id, MalFavoriteType Type);