// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

namespace PaperMalKing.AniList.Wrapper.GraphQL;

internal static class Queries
{
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