// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.Text;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;

namespace PaperMalKing.AniList.Wrapper.GraphQL;

internal static class Helpers
{
	private const string Media =
								"""
								title {
									stylisedRomaji: romaji(stylised: true)
									romaji(stylised: false)
									stylisedEnglish: english(stylised: true)
									english(stylised: false)
									stylisedNative: native(stylised: true)
									native(stylised: false)
								}
								countryOfOrigin
								type
								siteUrl
								id
								image: coverImage {
									large: extraLarge
								}
								""";

	internal static StringBuilder AppendMediaFields(StringBuilder sb, RequestOptions options)
	{
		sb.AppendLine(Media);
		if ((options & RequestOptions.MediaDescription) != 0)
			sb.AppendLine("description(asHtml: false)");

		if ((options & RequestOptions.Genres) != 0)
			sb.AppendLine("genres");
		if ((options & RequestOptions.Tags) != 0)
			sb.AppendLine(
				"""
				tags{
					name
					rank
					isMediaSpoiler
				}
				""");

		if ((options & RequestOptions.MediaFormat) != 0)
			sb.AppendLine(
				"""
				format
				countryOfOrigin
				""");
		if ((options & RequestOptions.MediaStatus) != 0)
			sb.AppendLine("status(version: 2)");
		if ((options & RequestOptions.Genres) != 0)
			sb.AppendLine("genres");
		if ((options & RequestOptions.Tags) != 0)
			sb.AppendLine(
				"""
				tags{
					name
					rank
					isMediaSpoiler
				}
				""");
		if ((options & RequestOptions.Studio) != 0)
			sb.AppendLine(
				"""
				studios(sort: FAVOURITES_DESC, isMain: true){
						values: nodes{
						name
						siteUrl
						isAnimationStudio
					}
				}
				""");
		if ((options & RequestOptions.Mangaka) != 0 || (options & RequestOptions.Director) != 0)
			sb.AppendLine(
				"""
				staff(sort: [RELEVANCE, ID], page: 1, perPage: 4){
					values: edges{
						role
						node{
							name{
								full
								native
							}
						siteUrl
						}
					}
				}
				""");
		if ((options & RequestOptions.Seyu) != 0 && (options & RequestOptions.AnimeList) != 0)
		{
			sb.AppendLine( // We select node since without it anilist provides empty array in voice actors
				"""
				characters(perPage: 6, sort: [ROLE, RELEVANCE]) {
					values: edges {
						voiceActors(language: JAPANESE, sort: [RELEVANCE]) {
							siteUrl
							name {
								native
								full
							}
							image {
								large
							}
						}
						node {
							id
						}
					}
				}
				""");
		}
		return sb;
	}
}