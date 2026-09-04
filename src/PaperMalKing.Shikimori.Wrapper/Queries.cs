// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text;
using GraphQL;
using PaperMalKing.Shikimori.Wrapper.Abstractions;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Enums;

namespace PaperMalKing.Shikimori.Wrapper;

internal static class Queries
{
	public static readonly CompositeFormat UserByIdQuery = CompositeFormat.Parse(
		"""
		query {{
			users (ids: [{0}], limit: 1) {{
				id,
				nickname,
				avatarUrl
			}}
		}}
		""");

	public static readonly GraphQLQuery UserByNicknameQuery = new(
		"""
		query ($nickname: String) {
			users (search: $nickname, limit: 1) {
				id,
				nickname,
				avatarUrl
			}
		}
		""");

	private const string GenresSubQuery = "genres { name russian }";

	private const string DescriptionSubQuery = "description";

	private const string PosterSubQuery =
		"""
		poster {
			main2xUrl,
			mainAlt2xUrl,
			mainAltUrl,
			mainUrl,
			mini2xUrl,
			miniAlt2xUrl,
			miniAltUrl,
			miniUrl,
			originalUrl,
			preview2xUrl,
			previewAlt2xUrl,
			previewAltUrl,
			previewUrl
		}
		""";

	public static string GetMangaQuery(RequestOptions options)
	{
		return $$"""
				query ($ids: String) {
					media: mangas (ids: $ids) {
						{{(options.HasFlag(RequestOptions.Publisher) ? " publishers { name, id } " : "")}}
						{{(options.HasFlag(RequestOptions.Mangaka) ? " personRoles { roles_russian: rolesRu, roles: rolesEn, person { name, russian, id, isMangaka } } " : "")}}
						{{(options.HasFlag(RequestOptions.Genres) ? GenresSubQuery : "")}}
						{{(options.HasFlag(RequestOptions.Description) ? DescriptionSubQuery : "")}}
						{{PosterSubQuery}}
					} 
				}
				""";
	}

	public static string GetAnimeQuery(RequestOptions options)
	{
		return $$"""
				query ($ids: String) {
					media: animes (ids: $ids) {
						{{(options.HasFlag(RequestOptions.Studio) ? "studios { name, id }" : "")}}
						{{(options.HasFlag(RequestOptions.Director) ? "personRoles { roles_russian: rolesRu, roles: rolesEn, person { name, russian, id } }" : "")}}
						{{(options.HasFlag(RequestOptions.Genres) ? GenresSubQuery : "")}}
						{{(options.HasFlag(RequestOptions.Description) ? DescriptionSubQuery : "")}}
						{{PosterSubQuery}}
					}
				}
				""";
	}

	private const string SearchLimit = "50";

	private const string SearchIdentitySubQuery =
		"""
		id
		name
		russian
		english
		japanese
		synonyms
		kind
		score
		status
		airedOn { year }
		statusesStats { count }
		url
		""";

	public static string GetAnimeSearchQuery(AnimeKind? kind, bool includeNsfw)
	{
		return $$"""
				query ($search: String) {
					media: animes (search: $search, limit: {{SearchLimit}}{{SearchArguments(kind?.ToGraphQlKind(), includeNsfw)}}) {
						{{SearchIdentitySubQuery}}
						rating
						studios { name, id }
						personRoles { roles_russian: rolesRu, roles: rolesEn, person { name, russian, id } }
						{{GenresSubQuery}}
						{{DescriptionSubQuery}}
						{{PosterSubQuery}}
					}
				}
				""";
	}

	public static string GetMangaSearchQuery(MangaKind? kind, bool includeNsfw)
	{
		return $$"""
				query ($search: String) {
					media: mangas (search: $search, limit: {{SearchLimit}}{{SearchArguments(kind?.ToGraphQlKind(), includeNsfw)}}) {
						{{SearchIdentitySubQuery}}
						publishers { name, id }
						personRoles { roles_russian: rolesRu, roles: rolesEn, person { name, russian, id, isMangaka } }
						{{GenresSubQuery}}
						{{DescriptionSubQuery}}
						{{PosterSubQuery}}
					}
				}
				""";
	}

	private static string SearchArguments(string? kindToken, bool includeNsfw)
	{
		var censored = includeNsfw ? "false" : "true";
		var kindArgument = kindToken is null ? "" : $", kind: \"{kindToken}\"";
		return $", censored: {censored}{kindArgument}";
	}
}