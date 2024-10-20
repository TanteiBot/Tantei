// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using Microsoft.Extensions.Logging;
using PaperMalKing.AniList.Wrapper.Abstractions;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Responses;
using PaperMalKing.AniList.Wrapper.GraphQL;

namespace PaperMalKing.AniList.Wrapper;

internal sealed class AniListClient(GraphQLHttpClient _client, ILogger<AniListClient> _logger) : IAniListClient
{
	public async Task<InitialUserInfoResponse> GetInitialUserInfoAsync(string username, byte favouritesPage = 1, CancellationToken cancellationToken = default)
	{
		_logger.RequestingInitialInfo(username, favouritesPage);
		var request = Requests.GetUserInitialInfoByUsernameRequest(username, favouritesPage);
		var response = await _client.SendQueryAsync<InitialUserInfoResponse>(request, cancellationToken);
		return response.Data;
	}

	public async Task<CheckForUpdatesResponse> CheckForUpdatesAsync(uint userId, byte page, long activitiesTimeStamp, ushort perChunk, ushort chunk,
																	RequestOptions options, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		_logger.RequestingUpdatesCheck(userId, page);
		var request = Requests.CheckForUpdatesRequest(userId, page, activitiesTimeStamp, perChunk, chunk, options);
		var response = await _client.SendQueryAsync<CheckForUpdatesResponse>(request, cancellationToken);
		return response.Data;
	}

	public async Task<FavouritesResponse> FavouritesInfoAsync(byte page, uint[] animeIds, uint[] mangaIds, uint[] charIds, uint[] staffIds, uint[] studioIds,
															  RequestOptions options, CancellationToken cancellationToken = default)
	{
		if (animeIds is [] && mangaIds is [] && charIds is [] && staffIds is [] && studioIds is [])
		{
			return FavouritesResponse.Empty;
		}

		var request = Requests.FavouritesInfoRequest(page, animeIds, mangaIds, charIds, staffIds, studioIds, options);
		var response = await _client.SendQueryAsync<FavouritesResponse>(request, cancellationToken);
		return response.Data;
	}
}