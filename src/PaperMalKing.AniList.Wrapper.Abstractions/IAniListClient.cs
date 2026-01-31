// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Responses;

namespace PaperMalKing.AniList.Wrapper.Abstractions;

public interface IAniListClient
{
	Task<InitialUserInfoResponse> GetInitialUserInfoAsync(string username, byte favouritesPage = 1, CancellationToken cancellationToken = default);

	Task<CheckForUpdatesResponse> CheckForUpdatesAsync(uint userId, byte page, long activitiesTimeStamp, ushort perChunk, ushort chunk, RequestOptions options, CancellationToken cancellationToken);

	Task<FavouritesResponse> FavouritesInfoAsync(byte page, uint[] animeIds, uint[] mangaIds, uint[] charIds, uint[] staffIds, uint[] studioIds,
												 RequestOptions options, CancellationToken cancellationToken);

	Task<MediaSearchResponse> SearchMediaAsync(string query, ListType mediaType, RequestOptions requestOptions, uint? userId, CancellationToken cancellationToken);
}