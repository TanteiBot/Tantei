// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text;

namespace PaperMalKing.AniList.Wrapper.GraphQL;

internal static class Queries
{
	public static string MediaByIdWithSeyuQuery { get; } = BuildMediaByIdWithSeyuQuery();

	private static string BuildMediaByIdWithSeyuQuery() =>
		new StringBuilder()
			.AppendLine("query ($id: Int, $type: MediaType) {")
			.AppendLine("Media(id: $id, type: $type) {")
			.AppendLine(Helpers.Media)
			.AppendLine(Helpers.CharactersWithVoiceActors)
			.AppendLine("}")
			.AppendLine("}")
			.ToString();

	public const string GetUserInitialInfoByUsernameQuery = """
	query ($username: String, $favouritePage: Int) {
		User(name: $username) {
			id
			siteUrl
			favourites {
				anime(page: $favouritePage, perPage: 25) {
					pageInfo {
						hasNextPage
					}
					values: nodes {
						id
					}
				}
				manga(page: $favouritePage, perPage: 25) {
					pageInfo {
						hasNextPage
					}
					values: nodes {
						id
					}
				}
				characters(page: $favouritePage, perPage: 25) {
					pageInfo {
						hasNextPage
					}
					values: nodes {
						id
					}
				}
				staff(page: $favouritePage, perPage: 25) {
					pageInfo {
						hasNextPage
					}
					values: nodes {
						id
					}
				}
				studios(page: $favouritePage, perPage: 25) {
					pageInfo {
						hasNextPage
					}
					values: nodes {
						id
					}
				}
			}
		}
	}
	""";
}