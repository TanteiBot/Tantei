// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

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
		if (options.HasFlag(RequestOptions.MediaDescription))
		{
			sb.AppendLine("description(asHtml: false)");
		}

		if (options.HasFlag(RequestOptions.Genres))
		{
			sb.AppendLine("genres");
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

		if (options.HasFlag(RequestOptions.MediaFormat))
		{
			sb.AppendLine(
				"""
				format
				countryOfOrigin
				""");
		}

		if (options.HasFlag(RequestOptions.MediaStatus))
		{
			sb.AppendLine("status(version: 2)");
		}

		if (options.HasFlag(RequestOptions.Genres))
		{
			sb.AppendLine("genres");
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

		if (options.HasFlag(RequestOptions.Studio))
		{
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
		}

		if (options.HasFlag(RequestOptions.Mangaka) || options.HasFlag(RequestOptions.Director))
		{
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
		}

		if (options.HasFlag(RequestOptions.Seyu) && options.HasFlag(RequestOptions.AnimeList))
		{
			sb.AppendLine(// We select node since without it anilist provides empty array in voice actors
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