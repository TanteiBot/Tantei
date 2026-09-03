// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;

namespace PaperMalKing.AniList.Wrapper.GraphQL;

internal static class MediaSearchQueryBuilder
{
	private const string UserBlock =
		"""
		User(id: $userId) {
			id
			name
			siteUrl
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
		}
		""";

	private const string PageHeader =
		"""
		Page(page: 1, perPage: 50) {
			values: media(search: $query, type: $type, sort: [SEARCH_MATCH]){
		""";

	private const string PageHeaderWithFormat =
		"""
		Page(page: 1, perPage: 50) {
			values: media(search: $query, type: $type, sort: [SEARCH_MATCH], format_in: $formatIn){
		""";

	private const string MediaCore =
		"""
		id
		title {
			stylisedRomaji: romaji(stylised: true)
			romaji(stylised: false)
			stylisedEnglish: english(stylised: true)
			english(stylised: false)
			stylisedNative: native(stylised: true)
			native(stylised: false)
		}
		synonyms
		type
		siteUrl
		image: coverImage {
			large: extraLarge
		}
		format
		isAdult
		popularity
		status(version: 2)
		episodes
		chapters
		volumes
		averageScore
		seasonYear
		season
		""";

	private const string Ender =
		"""
		}
		}
		}
		""";

	public static string BuildWithUser(RequestOptions options, bool hasFormat) => BuildQuery(options, hasFormat, includeUser: true);

	public static string Build(RequestOptions options, bool hasFormat) => BuildQuery(options, hasFormat, includeUser: false);

	private static string BuildQuery(RequestOptions options, bool hasFormat, bool includeUser)
	{
		var sb = new StringBuilder("query ($query: String, $type: MediaType");
		if (hasFormat)
		{
			sb.Append(", $formatIn: [MediaFormat]");
		}

		if (includeUser)
		{
			sb.Append(", $userId: Int");
		}

		sb.AppendLine("){").AppendLine();

		if (includeUser)
		{
			sb.AppendLine(UserBlock);
		}

		sb.AppendLine(hasFormat ? PageHeaderWithFormat : PageHeader);

		AppendSearchMediaFields(sb, options);

		sb.Append(Ender);

		return sb.ToString();
	}

	private static void AppendSearchMediaFields(StringBuilder sb, RequestOptions options)
	{
		sb.AppendLine(MediaCore);
		if (options.HasFlag(RequestOptions.MediaDescription))
		{
			sb.AppendLine("description(asHtml: false)");
		}

		if (options.HasFlag(RequestOptions.Tags))
		{
			sb.AppendLine(
				"""
				tags{
					name
					rank
					isMediaSpoiler
				}
				""");
		}
	}
}
