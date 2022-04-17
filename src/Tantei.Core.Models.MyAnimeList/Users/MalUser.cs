// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using Tantei.Core.Models.Users;

namespace Tantei.Core.Models.MyAnimeList.Users;

public sealed record MalUser(BotUser BotUser, ulong BotUserId, ulong Id, string Username, DateTimeOffset LastUpdatedAnimeTimeStamp,
							 DateTimeOffset LastUpdatedMangaTimeStamp, string LastAnimeUpdateHash, string LastMangaUpdateHash,
							 MalUserFeatures Features) : IUpdateProviderUser
{
	public IList<MalUserFavorite> FavoriteAnime { get; init; } = Array.Empty<MalUserFavorite>();
}