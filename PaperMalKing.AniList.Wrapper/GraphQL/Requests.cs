// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using GraphQL;
using PaperMalKing.AniList.Wrapper.Models;

namespace PaperMalKing.AniList.Wrapper.GraphQL;

internal static class Requests
{
	public static GraphQLRequest GetUserInitialInfoByUsernameRequest(string username, byte favouritePage) =>
		new(Queries.GetUserInitialInfoByUsernameQuery, new {username = username, favouritePage = favouritePage});

	public static GraphQLRequest CheckForUpdatesRequest(ulong userId, byte page, long activityTimeStamp, ushort perChunk, ushort chunk,
														RequestOptions options) =>
		new(UpdateCheckQueryBuilder.Build(options),
			new {userId = userId, page = page, activitiesFilterTimeStamp = activityTimeStamp, perChunk = perChunk, chunk = chunk});

	public static GraphQLRequest FavouritesInfoRequest(byte page, ulong[] animeIds, ulong[] mangaIds, ulong[] charIds, ulong[] staffIds,
													   ulong[] studioIds, RequestOptions options) =>
		new(FavouritesInfoQueryBuilder.Build(options),
			new {page = page, animeIds = animeIds, mangaIds = mangaIds, charIds = charIds, staffIds = staffIds, studioIds = studioIds});
}