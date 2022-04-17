// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace Tantei.Core.Models.Shikimori.Users;

public sealed record ShikimoriUserFavorite(ulong Id, ShikimoriUserFavoriteType Type, ulong UserId, ShikimoriUser User);