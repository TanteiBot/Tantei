// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using GraphQL;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

namespace PaperMalKing.AniList.Wrapper.GraphQL;

internal static class Requests
{
	public static GraphQLRequest GetUserInitialInfoByUsernameRequest(string username, byte favouritePage) =>
		new(Queries.GetUserInitialInfoByUsernameQuery, new
		{
			username,
			favouritePage,
		});

	public static GraphQLRequest CheckForUpdatesRequest(uint userId, byte page, long activityTimeStamp, ushort perChunk, ushort chunk, RequestOptions options) =>
		new(
			UpdateCheckQueryBuilder.Build(options),
			new
			{
				userId,
				page,
				activitiesFilterTimeStamp = activityTimeStamp,
				perChunk,
				chunk,
			});

	public static GraphQLRequest FavouritesInfoRequest(byte page, uint[] animeIds, uint[] mangaIds, uint[] charIds, uint[] staffIds, uint[] studioIds, RequestOptions options) =>
		new(
			FavouritesInfoQueryBuilder.Build(options),
			new
			{
				page,
				animeIds,
				mangaIds,
				charIds,
				staffIds,
				studioIds,
			});

	public static GraphQLRequest SearchMediaRequest(string query, RequestOptions options, ListType type, MediaFormat? format, uint? userId)
	{
		var hasFormat = format is not null;
		var hasUser = userId is not null;

		var variables = new Dictionary<string, object?>(StringComparer.Ordinal)
		{
			["query"] = query,
			["type"] = type,
		};
		if (hasFormat)
		{
			variables["formatIn"] = new[] { format!.Value };
		}

		if (hasUser)
		{
			variables["userId"] = userId;
		}

		var queryText = hasUser
			? MediaSearchQueryBuilder.BuildWithUser(options, hasFormat)
			: MediaSearchQueryBuilder.Build(options, hasFormat);

		return new(queryText, variables);
	}
}