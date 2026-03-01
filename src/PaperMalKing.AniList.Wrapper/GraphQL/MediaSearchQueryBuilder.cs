// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;

namespace PaperMalKing.AniList.Wrapper.GraphQL;

internal static class MediaSearchQueryBuilder
{
	private const string UserSmallInfo =
		"""
		User(id: $userId) {
			id
			name
			options {
				titleLanguage
			}
			siteUrl
		}
		""";

	private const string MediaHeader =
		"""
		Media(search: $query, type: $type){
		""";

	private const string Ender =
		"""
		}
		}
		""";

	public static string BuildWithUser(RequestOptions options)
	{
		var sb = new StringBuilder(
			"""
			query ($query: String, $type: MediaType, $userId: Int){
				
			""");

		sb.Append(UserSmallInfo)
		  .Append(MediaHeader);

		Helpers.AppendMediaFields(sb, options);

		sb.Append(Ender);

		return sb.ToString();
	}

	public static string Build(RequestOptions options)
	{
		var sb = new StringBuilder(
			"""
			query ($query: String, $type: MediaType){
				
			""");

		sb.Append(MediaHeader);

		Helpers.AppendMediaFields(sb, options);

		sb.Append(Ender);

		return sb.ToString();
	}
}