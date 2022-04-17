// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using Tantei.Core.Models.Users;

namespace Tantei.Core.Models.Shikimori.Users;

public sealed record ShikimoriUser
	(ulong Id, ulong LastHistoryEntryId, ulong BotUserId, BotUser BotUser, ShikimoriUserFeatures Features) : IUpdateProviderUser
{
	public IList<ShikimoriUserFavorite> Favorites { get; init; } = Array.Empty<ShikimoriUserFavorite>();
}