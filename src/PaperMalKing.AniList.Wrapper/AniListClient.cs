// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using Microsoft.Extensions.Logging;
using PaperMalKing.AniList.Wrapper.Abstractions;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Responses;
using PaperMalKing.AniList.Wrapper.GraphQL;

namespace PaperMalKing.AniList.Wrapper;

internal sealed class AniListClient : IAniListClient
{
	private readonly GraphQLHttpClient _client;
	private readonly ILogger<AniListClient> _logger;

	public AniListClient(GraphQLHttpClient client, ILogger<AniListClient> logger)
	{
		this._client = client;
		this._logger = logger;
	}

	public async Task<InitialUserInfoResponse> GetInitialUserInfoAsync(string username, byte favouritesPage = 1,
																		 CancellationToken cancellationToken = default)
	{
		this._logger.LogDebug("Requesting initial info for {Username}, {Page}", username, favouritesPage);
		var request = Requests.GetUserInitialInfoByUsernameRequest(username, favouritesPage);
		var response = await this._client.SendQueryAsync<InitialUserInfoResponse>(request, cancellationToken).ConfigureAwait(false);
		return response.Data;
	}

	public async Task<CheckForUpdatesResponse> CheckForUpdatesAsync(uint userId, byte page, long activitiesTimeStamp, ushort perChunk,
																	ushort chunk, RequestOptions options, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		this._logger.LogDebug("Requesting updates check for {UserId}, {Page}", userId, page);
		var request = Requests.CheckForUpdatesRequest(userId, page, activitiesTimeStamp, perChunk, chunk, options);
		var response = await this._client.SendQueryAsync<CheckForUpdatesResponse>(request, cancellationToken).ConfigureAwait(false);
		return response.Data;
	}

	public async Task<FavouritesResponse> FavouritesInfoAsync(byte page, uint[] animeIds, uint[] mangaIds, uint[] charIds, uint[] staffIds,
															  uint[] studioIds, RequestOptions options, CancellationToken cancellationToken = default)
	{
		if (animeIds.Length == 0 && mangaIds.Length == 0 && charIds.Length == 0 && staffIds.Length == 0 && studioIds.Length == 0)
			return FavouritesResponse.Empty;

		var request = Requests.FavouritesInfoRequest(page, animeIds, mangaIds, charIds, staffIds, studioIds, options);
		var response = await this._client.SendQueryAsync<FavouritesResponse>(request, cancellationToken).ConfigureAwait(false);
		return response.Data;
	}
}