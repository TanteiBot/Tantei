// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using System.Text;
using GraphQL;
using PaperMalKing.Shikimori.Wrapper.Abstractions;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;
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

	private const string EntityPosterSubQuery =
		"""
		poster {
			main2xUrl,
			mainUrl,
			mini2xUrl,
			miniUrl,
			originalUrl,
			preview2xUrl,
			previewUrl
		}
		""";

	public static string GetMangaQuery(RequestOptions options)
	{
		return $$"""
				query ($ids: String) {
					media: mangas (ids: $ids) {
						{{(options.HasFlag(RequestOptions.Publisher) ? " publishers { name, id } " : "")}}
						{{(options.HasFlag(RequestOptions.Mangaka) ? MangakaSubQuery : "")}}
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
						{{(options.HasFlag(RequestOptions.Director) ? DirectorSubQuery : "")}}
						{{(options.HasFlag(RequestOptions.Genres) ? GenresSubQuery : "")}}
						{{(options.HasFlag(RequestOptions.Description) ? DescriptionSubQuery : "")}}
						{{PosterSubQuery}}
					}
				}
				""";
	}

	private const string DirectorSubQuery = "personRoles { roles_russian: rolesRu, roles: rolesEn, person { name, russian, id } }";

	private const string MangakaSubQuery = "personRoles { roles_russian: rolesRu, roles: rolesEn, person { name, russian, id, isMangaka } }";

	public const int FavouritesBatchLimit = 50;

	private const string FavouritesLimit = "50";

	private const string MediaIdentitySubQuery =
		"""
		id
		name
		russian
		kind
		status
		score
		url
		""";

	public static string GetFavouritesInfoQuery(FavouriteIds ids, RequestOptions options)
	{
		ArgumentNullException.ThrowIfNull(ids);

		var sb = new StringBuilder("query {");

		if (ids.AnimeIds.Count > 0)
		{
			sb.AppendLine().Append(AnimesAliasSubQuery(ids.AnimeIds, options));
		}

		if (ids.MangaIds.Count > 0)
		{
			sb.AppendLine().Append(MangasAliasSubQuery(ids.MangaIds, options));
		}

		if (ids.CharacterIds.Count > 0)
		{
			sb.AppendLine().Append(CharactersAliasSubQuery(ids.CharacterIds, options));
		}

		if (ids.PersonIds.Count > 0)
		{
			sb.AppendLine().Append(PeopleAliasSubQuery(ids.PersonIds));
		}

		return sb.AppendLine().Append('}').ToString();
	}

	private static string AnimesAliasSubQuery(IReadOnlyList<uint> ids, RequestOptions options)
	{
		return $$"""
				animes (ids: "{{JoinIds(ids)}}", limit: {{FavouritesLimit}}) {
					{{MediaIdentitySubQuery}}
					episodes
					episodesAired
					{{(options.HasFlag(RequestOptions.Studio) ? "studios { name, id }" : "")}}
					{{(options.HasFlag(RequestOptions.Director) ? DirectorSubQuery : "")}}
					{{SharedMediaEnrichmentSubQuery(options)}}
					{{PosterSubQuery}}
				}
				""";
	}

	private static string MangasAliasSubQuery(IReadOnlyList<uint> ids, RequestOptions options)
	{
		return $$"""
				mangas (ids: "{{JoinIds(ids)}}", limit: {{FavouritesLimit}}) {
					{{MediaIdentitySubQuery}}
					chapters
					volumes
					{{(options.HasFlag(RequestOptions.Publisher) ? "publishers { name, id }" : "")}}
					{{(options.HasFlag(RequestOptions.Mangaka) ? MangakaSubQuery : "")}}
					{{SharedMediaEnrichmentSubQuery(options)}}
					{{PosterSubQuery}}
				}
				""";
	}

	private static string CharactersAliasSubQuery(IReadOnlyList<uint> ids, RequestOptions options)
	{
		return $$"""
				characters (ids: "{{JoinIds(ids)}}", limit: {{FavouritesLimit}}) {
					id
					name
					russian
					url
					{{(options.HasFlag(RequestOptions.Description) ? DescriptionSubQuery : "")}}
					{{EntityPosterSubQuery}}
				}
				""";
	}

	private static string PeopleAliasSubQuery(IReadOnlyList<uint> ids)
	{
		return $$"""
				people (ids: [{{JoinQuotedIds(ids)}}], limit: {{FavouritesLimit}}) {
					id
					name
					russian
					url
					isMangaka
					isProducer
					isSeyu
					{{EntityPosterSubQuery}}
				}
				""";
	}

	private static string SharedMediaEnrichmentSubQuery(RequestOptions options)
	{
		var genres = options.HasFlag(RequestOptions.Genres) ? GenresSubQuery : "";
		var description = options.HasFlag(RequestOptions.Description) ? DescriptionSubQuery : "";
		return genres + Environment.NewLine + description;
	}

	private static string JoinIds(IReadOnlyList<uint> ids) => string.Join(',', ids.Select(static id => id.ToString(CultureInfo.InvariantCulture)));

	private static string JoinQuotedIds(IReadOnlyList<uint> ids) =>
		string.Join(',', ids.Select(static id => $"\"{id.ToString(CultureInfo.InvariantCulture)}\""));

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
						{{DirectorSubQuery}}
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
						{{MangakaSubQuery}}
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
