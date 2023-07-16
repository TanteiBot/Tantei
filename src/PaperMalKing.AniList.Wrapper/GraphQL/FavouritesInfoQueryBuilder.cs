// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.Text;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;

namespace PaperMalKing.AniList.Wrapper.GraphQL;

internal static class FavouritesInfoQueryBuilder
{
	public static string Build(RequestOptions options)
	{
		var sb = new StringBuilder();
		sb.AppendLine(
			"""
			query ($page: Int, $animeIds: [Int], $mangaIds: [Int], $charIds: [Int], $staffIds: [Int], $studioIds: [Int]) {
				Animes: Page(page: $page, perPage: 50) {
					pageInfo{
						hasNextPage
					}
			values: media(type: ANIME, id_in: $animeIds) {
			""");
		Helpers.AppendMediaFields(sb, options);
		sb.AppendLine(
			"""
			}
			}
			Mangas: Page(page: $page, perPage: 50) {
				pageInfo{
					hasNextPage
				}
				values: media(type: MANGA, id_in: $mangaIds) {
			""");
		Helpers.AppendMediaFields(sb, options);
		sb.AppendLine(
			"""
			}
			}
			""")
		  .AppendLine(
		  	"""
			Staff: Page(page: $page, perPage: 50) {
				pageInfo{
					hasNextPage
				}
				values: staff(id_in: $staffIds) {
					name {
						full
						native
					}
					id
					primaryOccupations,
					staffMedia(sort:POPULARITY_DESC, page: 1, perPage: 1){
			""");
		FillLesserMediaFields(sb);
		sb.Append(
			"""
			}
			siteUrl
			image {
				large
			}
			""");
		if (options.HasFlag(RequestOptions.MediaDescription))
			sb.AppendLine("description(asHtml: false)");

		sb.AppendLine(
			"""
			}
			}
			""")
			.AppendLine(
			"""
			Characters: Page(page: $page, perPage: 50) {
				pageInfo{
					hasNextPage
				}
				values: characters(id_in: $charIds) {
					name {
						full
						native
					}
					siteUrl
					id
					image {
					large
				}
			media(sort: POPULARITY_DESC, page: 1, perPage: 1) {
			""");
		FillLesserMediaFields(sb);
		sb.Append(
			"""
			}
			}
			}
			Studios: Page(page: $page, perPage: 50) {
				pageInfo{
					hasNextPage
				}
				values: studios(id_in: $studioIds) {
					name
					siteUrl
					id
					media(sort: POPULARITY_DESC, isMain: true, page: 1, perPage: 1) {
			""");

		FillLesserMediaFields(sb);
		sb.Append(
			"""
			}
			}
			}
			}
			""");
		return sb.ToString();
	}

	private static void FillLesserMediaFields(StringBuilder sb)
	{
		sb.Append(
			"""
			values: nodes {
				title {
					stylisedRomaji: romaji(stylised: true)
					romaji(stylised: false)
					stylisedEnglish: english(stylised: true)
					english(stylised: false)
					stylisedNative: native(stylised: true)
					native(stylised: false)
				}
			siteUrl
			format
			}						
			""");
	}
}