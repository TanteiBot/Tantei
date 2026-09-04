// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.Common.Enums;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions;

public interface IShikiClient
{
	Task<UserInfo> GetUserByNicknameAsync(string nickname, CancellationToken cancellationToken);

	Task<UserInfo> GetUserByIdAsync(uint userId, CancellationToken cancellationToken);

	Task<Favourites> GetUserFavouritesAsync(uint userId, CancellationToken cancellationToken);

	Task<Paginatable<History>> GetUserHistoryAsync(uint userId, uint page, byte limit, HistoryRequestOptions options, CancellationToken cancellationToken);

	Task<TMedia?> GetMediaAsync<TMedia>(ulong id, ListEntryType type, RequestOptions options, CancellationToken cancellationToken)
		where TMedia : BaseMedia;

	Task<IReadOnlyList<UserAchievement>> GetUserAchievementsAsync(uint userId, CancellationToken cancellationToken);

	Task<IReadOnlyList<AnimeSearchMedia>> SearchAnimeAsync(string query, AnimeKind? kind, bool includeNsfw, CancellationToken cancellationToken);

	Task<IReadOnlyList<MangaSearchMedia>> SearchMangaAsync(string query, MangaKind? kind, bool includeNsfw, CancellationToken cancellationToken);

	Task<byte[]?> GetImageContentAsync(string url, CancellationToken cancellationToken);
}