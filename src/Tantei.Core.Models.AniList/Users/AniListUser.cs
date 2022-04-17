// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using Tantei.Core.Models.Users;

namespace Tantei.Core.Models.AniList.Users;

public sealed record AniListUser(ulong Id, BotUser BotUser, ulong BotUserId, ulong LastListActivityTimeStamp, ulong LastReviewTimeStamp,
								 AniListUserFeatures Features) : IUpdateProviderUser
{
	public IList<AniListFavorite> Favorites { get; init; } = Array.Empty<AniListFavorite>();
}