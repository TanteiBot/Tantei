// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Globalization;
using System.Text;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;

namespace PaperMalKing.AniList.Wrapper.GraphQL;

internal static class UpdateCheckQueryBuilder
{
	private const string QueryStart =
		"""
		query ($userId: Int, $page: Int, $activitiesFilterTimeStamp: Int, $perChunk: Int, $chunk: Int) {
			User(id: $userId) {
				id
				name
				siteUrl
				image: avatar {
					large
				}
				options {
					titleLanguage
				}
				mediaListOptions {
					scoreFormat
					animeList {
						advancedScoringEnabled
					}
					mangaList {
						advancedScoringEnabled
					}
				}
		""";

	private const string FavouritesSubQuery =
			"""
			favourites {
				anime(page: $page, perPage: 25) {
					pageInfo {
						hasNextPage
					}
					values: nodes {
						id
					}
				}
				manga(page: $page, perPage: 25) {
					pageInfo {
						hasNextPage
					}
					values: nodes {
						id
					}
				}
				characters(page: $page, perPage: 25) {
					pageInfo {
						hasNextPage
					}
					values: nodes {
						id
					}
				}
				staff(page: $page, perPage: 25) {
					pageInfo {
						hasNextPage
					}
					values: nodes {
						id
					}
				}
				studios(page: $page, perPage: 25) {
					pageInfo {
						hasNextPage
					}
					values: nodes {
						id
					}
				}
			}
			""";

	private static readonly CompositeFormat AnimeListSubQuery = CompositeFormat.Parse(
		"""
		AnimeList: MediaListCollection(userId: $userId, type: ANIME, sort: UPDATED_TIME_DESC, chunk: $chunk, perChunk: $perChunk) {{
			lists {{
				entries {{
					id: mediaId
					status
					point100Score: score(format: POINT_100)
					point10DecimalScore: score(format: POINT_10_DECIMAL)
					point10Score: score(format: POINT_10)
					point5Score: score(format: POINT_5)
					point3Score: score(format: POINT_3)
					repeat
					notes
					advancedScores
					{0}
				}}
			}}
		}}
		""");

	private static readonly CompositeFormat MangaListSubQuery = CompositeFormat.Parse(
		"""
		MangaList: MediaListCollection(userId: $userId, type: MANGA, sort: UPDATED_TIME_DESC, chunk: $chunk, perChunk: $perChunk) {{
			lists {{
				entries {{
					id: mediaId
					status
					point100Score: score(format: POINT_100)
					point10DecimalScore: score(format: POINT_10_DECIMAL)
					point10Score: score(format: POINT_10)
					point5Score: score(format: POINT_5)
					point3Score: score(format: POINT_3)
					repeat
					notes
					advancedScores
					{0}
				}}
			}}
		}}
		""");

	private const string ReviewsSubQuery =
		"""
		ReviewsPage: Page(page: $page, perPage: 50) {
			pageInfo {
				hasNextPage
			}
			values: reviews(userId: $userId, sort: CREATED_AT_DESC) {
				createdAt
				siteUrl
				summary
				media {
					title {
						stylisedRomaji: romaji(stylised: true)
						romaji(stylised: false)
						stylisedEnglish: english(stylised: true)
						english(stylised: false)
						stylisedNative: native(stylised: true)
						native(stylised: false)
					}
					format
					image: coverImage {
						large: extraLarge
					}
				}
			}
		}
		""";

	public static string Build(RequestOptions options)
	{
		var hasAnime = options.HasFlag(RequestOptions.AnimeList);
		var hasManga = options.HasFlag(RequestOptions.MangaList);
		var hasFavs = options.HasFlag(RequestOptions.Favourites);

		var hasReview = options.HasFlag(RequestOptions.Reviews);
		var sb = new StringBuilder(QueryStart);
		if (hasFavs)
		{
			sb.AppendLine(FavouritesSubQuery);
		}
		sb.AppendLine("}");
		if (hasAnime)
		{
			sb.AppendFormat(CultureInfo.InvariantCulture, AnimeListSubQuery,
				options.HasFlag(RequestOptions.CustomLists) ? "customLists(asArray: true)" : "").AppendLine();
		}

		if (hasManga)
		{
			sb.AppendFormat(CultureInfo.InvariantCulture, MangaListSubQuery,
				options.HasFlag(RequestOptions.CustomLists) ? "customLists(asArray: true)" : "").AppendLine();
		}
		if (hasAnime || hasManga)
		{
			sb.AppendLine(
				"""
				ActivitiesPage: Page(page: $page, perPage: 50) {
					pageInfo {
						hasNextPage
					}
				""");
			var type = hasAnime switch
			{
				true when hasManga => "MEDIA_LIST",
				true => "ANIME_LIST",
				_ => "MANGA_LIST"
			};
			sb.AppendLine(CultureInfo.InvariantCulture, $"values: activities(userId: $userId, type: {type}, sort: ID_DESC, createdAt_greater: $activitiesFilterTimeStamp) {{")
				.AppendLine(
				"""
				... on ListActivity {
					status
					progress
					createdAt
					media {
				""");
			if (hasAnime)
				sb.AppendLine("episodes");
			if (hasManga)
			{
				sb.AppendLine(
					"""
					chapters
					volumes
					""");
			}

			Helpers.AppendMediaFields(sb, options);
			sb.AppendLine(
				"""
				}
				}
				}
				}
				""");
		}
		if (hasReview)
		{
			sb.AppendLine(ReviewsSubQuery);
		}
		sb.AppendLine("}");
		return sb.ToString();
	}
}