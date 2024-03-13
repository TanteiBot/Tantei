// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PaperMalKing.Common.Enums;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions;

public interface IShikiClient
{
	Task<UserInfo> GetUserAsync(string nickname, CancellationToken cancellationToken = default);

	Task<Favourites> GetUserFavouritesAsync(uint userId, CancellationToken cancellationToken = default);

	Task<Paginatable<History[]>> GetUserHistoryAsync(uint userId, uint page, byte limit, HistoryRequestOptions options, CancellationToken cancellationToken = default);

	Task<TMedia?> GetMediaAsync<TMedia>(ulong id, ListEntryType type, CancellationToken cancellationToken = default)
		where TMedia : BaseMedia;

	Task<IReadOnlyList<Role>> GetMediaStaffAsync(ulong id, ListEntryType type, CancellationToken cancellationToken = default);

	Task<UserInfo> GetUserInfoAsync(uint userId, CancellationToken cancellationToken = default);

	Task<IReadOnlyList<UserAchievement>> GetUserAchievementsAsync(uint userId, CancellationToken cancellationToken = default);
}