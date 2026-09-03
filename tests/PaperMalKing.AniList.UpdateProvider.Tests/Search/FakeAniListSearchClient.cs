// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.AniList.Wrapper.Abstractions;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Responses;

namespace PaperMalKing.AniList.UpdateProvider.Tests.Search;

internal sealed class FakeAniListSearchClient : IAniListClient
{
	public MediaSearchResponse Response { get; init; } = MediaSearchResponse.Empty;

	public Exception? SearchException { get; init; }

	public int CallCount { get; private set; }

	public List<string> Queries { get; } = [];

	public List<ListType> Types { get; } = [];

	public List<RequestOptions> Options { get; } = [];

	public List<uint?> UserIds { get; } = [];

	public Task<MediaSearchResponse> SearchMediaAsync(string query, ListType mediaType, RequestOptions requestOptions, uint? userId, CancellationToken cancellationToken)
	{
		this.CallCount++;
		this.Queries.Add(query);
		this.Types.Add(mediaType);
		this.Options.Add(requestOptions);
		this.UserIds.Add(userId);
		return this.SearchException is null
			? Task.FromResult(this.Response)
			: Task.FromException<MediaSearchResponse>(this.SearchException);
	}

	public Task<InitialUserInfoResponse> GetInitialUserInfoAsync(string username, byte favouritesPage = 1, CancellationToken cancellationToken = default) =>
		throw new NotSupportedException();

	public Task<CheckForUpdatesResponse> CheckForUpdatesAsync(
		uint userId,
		byte page,
		long activitiesTimeStamp,
		ushort perChunk,
		ushort chunk,
		RequestOptions options,
		CancellationToken cancellationToken) =>
		throw new NotSupportedException();

	public Task<FavouritesResponse> FavouritesInfoAsync(
		byte page,
		uint[] animeIds,
		uint[] mangaIds,
		uint[] charIds,
		uint[] staffIds,
		uint[] studioIds,
		RequestOptions options,
		CancellationToken cancellationToken) =>
		throw new NotSupportedException();
}
