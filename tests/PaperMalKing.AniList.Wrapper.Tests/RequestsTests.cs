// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.AniList.Wrapper.GraphQL;

namespace PaperMalKing.AniList.Wrapper.Tests;

public sealed class RequestsTests
{
	internal const RequestOptions AnimeAndMangaLists = RequestOptions.AnimeList | RequestOptions.MangaList;

	[Test]
	[Arguments(RequestOptions.Favourites, "favourites {")]
	[Arguments(RequestOptions.Reviews, "ReviewsPage: Page(")]
	[Arguments(RequestOptions.CustomLists, "customLists(asArray: true)")]
	[Arguments(RequestOptions.MediaDescription, "description(asHtml: false)")]
	[Arguments(RequestOptions.MediaStatus, "status(version: 2)")]
	[Arguments(RequestOptions.Studio, "studios(sort: FAVOURITES_DESC")]
	[Arguments(RequestOptions.Director, "staff(sort: [RELEVANCE, ID]")]
	[Arguments(RequestOptions.Mangaka, "staff(sort: [RELEVANCE, ID]")]
	[Arguments(RequestOptions.Seyu, "voiceActors(language: JAPANESE")]
	public async Task UpdateCheckQueryEmitsBlockOnlyWhenFlagPresent(RequestOptions flag, string marker)
	{
		var withFlag = UpdateCheckQueryBuilder.Build(AnimeAndMangaLists | flag);
		var withoutFlag = UpdateCheckQueryBuilder.Build(AnimeAndMangaLists);

		await Assert.That(withFlag).Contains(marker);
		await Assert.That(withoutFlag).DoesNotContain(marker);
		GraphQlAssertions.AssertValidGraphQl(withFlag);
	}

	[Test]
	public async Task UpdateCheckQueryEmitsAnimeListBlockOnlyWithAnimeListFlag()
	{
		var withAnime = UpdateCheckQueryBuilder.Build(RequestOptions.AnimeList);
		var mangaOnly = UpdateCheckQueryBuilder.Build(RequestOptions.MangaList);

		await Assert.That(withAnime).Contains("AnimeList: MediaListCollection(userId: $userId, type: ANIME,");
		await Assert.That(mangaOnly).DoesNotContain("AnimeList: MediaListCollection");
		GraphQlAssertions.AssertValidGraphQl(withAnime);
	}

	[Test]
	public async Task UpdateCheckQueryEmitsMangaListBlockOnlyWithMangaListFlag()
	{
		var withManga = UpdateCheckQueryBuilder.Build(RequestOptions.MangaList);
		var animeOnly = UpdateCheckQueryBuilder.Build(RequestOptions.AnimeList);

		await Assert.That(withManga).Contains("MangaList: MediaListCollection(userId: $userId, type: MANGA,");
		await Assert.That(animeOnly).DoesNotContain("MangaList: MediaListCollection");
		GraphQlAssertions.AssertValidGraphQl(withManga);
	}

	[Test]
	[Arguments(RequestOptions.AnimeList, "type: ANIME_LIST")]
	[Arguments(RequestOptions.MangaList, "type: MANGA_LIST")]
	[Arguments(AnimeAndMangaLists, "type: MEDIA_LIST")]
	public async Task UpdateCheckQueryPicksActivityTypeToken(RequestOptions options, string expectedToken)
	{
		var query = UpdateCheckQueryBuilder.Build(options);

		var tokens = new[] { "type: ANIME_LIST", "type: MANGA_LIST", "type: MEDIA_LIST" };
		await Assert.That(query).Contains(expectedToken);
		foreach (var other in tokens.Where(t => !string.Equals(t, expectedToken, StringComparison.Ordinal)))
		{
			await Assert.That(query).DoesNotContain(other);
		}

		GraphQlAssertions.AssertValidGraphQl(query);
	}

	[Test]
	public async Task UpdateCheckQueryWithNoFlagsOmitsEveryOptionalBlock()
	{
		var query = UpdateCheckQueryBuilder.Build(default);

		foreach (var marker in new[]
		{
			"favourites {", "AnimeList: MediaListCollection", "MangaList: MediaListCollection", "ActivitiesPage: Page(", "ReviewsPage: Page(",
		})
		{
			await Assert.That(query).DoesNotContain(marker);
		}

		await Assert.That(query).Contains("User(id: $userId)");
		GraphQlAssertions.AssertValidGraphQl(query);
	}

	[Test]
	[Arguments(RequestOptions.Genres, "genres")]
	[Arguments(RequestOptions.Tags, "tags{")]
	public async Task UpdateCheckQueryEmitsMediaFieldExactlyOnce(RequestOptions flag, string marker)
	{
		var withFlag = UpdateCheckQueryBuilder.Build(AnimeAndMangaLists | flag);
		var withoutFlag = UpdateCheckQueryBuilder.Build(AnimeAndMangaLists);

		await Assert.That(OccurrenceCount(withoutFlag, marker)).IsEqualTo(0);
		await Assert.That(OccurrenceCount(withFlag, marker)).IsEqualTo(1);
		GraphQlAssertions.AssertValidGraphQl(withFlag);
	}

	[Test]
	public async Task UpdateCheckQueryEmitsCountryOfOriginOnceWithMediaFormat()
	{
		var withoutFormat = UpdateCheckQueryBuilder.Build(AnimeAndMangaLists);
		var withFormat = UpdateCheckQueryBuilder.Build(AnimeAndMangaLists | RequestOptions.MediaFormat);

		await Assert.That(OccurrenceCount(withoutFormat, "countryOfOrigin")).IsEqualTo(1);
		await Assert.That(OccurrenceCount(withFormat, "countryOfOrigin")).IsEqualTo(1);
		await Assert.That(withFormat).Contains("format");
		GraphQlAssertions.AssertValidGraphQl(withFormat);
	}

	[Test]
	public async Task ProfileQueryDeclaresVariablesAndSelectsFavourites()
	{
		var query = Requests.GetUserInitialInfoByUsernameRequest("N0D4N", 1).Query!;

		await Assert.That(query).Contains("query ($username: String, $favouritePage: Int)");
		await Assert.That(query).Contains("User(name: $username)");
		foreach (var block in new[]
		{
			"favourites {",
			"anime(page: $favouritePage, perPage: 25)",
			"manga(page: $favouritePage, perPage: 25)",
			"characters(page: $favouritePage, perPage: 25)",
			"staff(page: $favouritePage, perPage: 25)",
			"studios(page: $favouritePage, perPage: 25)",
		})
		{
			await Assert.That(query).Contains(block);
		}

		GraphQlAssertions.AssertValidGraphQl(query);
	}

	[Test]
	public async Task FavouritesInfoQueryEmitsDescriptionOnlyWithMediaDescriptionFlag()
	{
		var withDescription = FavouritesInfoQueryBuilder.Build(RequestOptions.MediaDescription);
		var withoutDescription = FavouritesInfoQueryBuilder.Build(default);

		await Assert.That(withDescription).Contains("description(asHtml: false)");
		await Assert.That(withoutDescription).DoesNotContain("description(asHtml: false)");
		GraphQlAssertions.AssertValidGraphQl(withDescription);
		GraphQlAssertions.AssertValidGraphQl(withoutDescription);
	}

	[Test]
	public async Task SearchMediaRequestSuppliesFormatInVariableOnlyWhenFormatGiven()
	{
		var withFormat = Requests.SearchMediaRequest("q", RequestOptions.AnimeList, ListType.Anime, MediaFormat.TV, userId: null);
		var withoutFormat = Requests.SearchMediaRequest("q", RequestOptions.AnimeList, ListType.Anime, format: null, userId: null);

		await Assert.That(VariablesOf(withFormat).ContainsKey("formatIn")).IsTrue();
		await Assert.That(withFormat.Query).Contains("format_in: $formatIn");
		await Assert.That(VariablesOf(withoutFormat).ContainsKey("formatIn")).IsFalse();
		await Assert.That(withoutFormat.Query).DoesNotContain("formatIn");
		GraphQlAssertions.AssertValidGraphQl(withFormat.Query!);
		GraphQlAssertions.AssertValidGraphQl(withoutFormat.Query!);
	}

	[Test]
	public async Task SearchMediaRequestSuppliesUserIdVariableAndUserBlockOnlyWhenUserGiven()
	{
		var withUser = Requests.SearchMediaRequest("q", RequestOptions.AnimeList, ListType.Anime, format: null, userId: 42u);
		var withoutUser = Requests.SearchMediaRequest("q", RequestOptions.AnimeList, ListType.Anime, format: null, userId: null);

		await Assert.That(VariablesOf(withUser).ContainsKey("userId")).IsTrue();
		await Assert.That(withUser.Query).Contains("User(id: $userId)");
		await Assert.That(VariablesOf(withoutUser).ContainsKey("userId")).IsFalse();
		await Assert.That(withoutUser.Query).DoesNotContain("$userId");
		GraphQlAssertions.AssertValidGraphQl(withUser.Query!);
		GraphQlAssertions.AssertValidGraphQl(withoutUser.Query!);
	}

	[Test]
	public async Task CheckForUpdatesRequestSuppliesExactlyTheVariablesItsQueryDeclares()
	{
		var request = Requests.CheckForUpdatesRequest(1u, 1, 0L, 1, 1, RequestOptions.All);

		await Assert.That(SuppliedVariableNames(request.Variables)).IsEqualTo(DeclaredVariableNames(request.Query!));
		GraphQlAssertions.AssertValidGraphQl(request.Query!);
	}

	[Test]
	public async Task FavouritesInfoRequestSuppliesExactlyTheVariablesItsQueryDeclares()
	{
		var ids = new[] { 1u };
		var request = Requests.FavouritesInfoRequest(1, ids, ids, ids, ids, ids, RequestOptions.All);

		await Assert.That(SuppliedVariableNames(request.Variables)).IsEqualTo(DeclaredVariableNames(request.Query!));
		GraphQlAssertions.AssertValidGraphQl(request.Query!);
	}

	[Test]
	public async Task UpdateCheckQueryEmitsVoiceActorsOnlyWhenSeyuAccompaniesAnimeList()
	{
		var seyuWithoutAnime = UpdateCheckQueryBuilder.Build(RequestOptions.MangaList | RequestOptions.Seyu);

		await Assert.That(seyuWithoutAnime).DoesNotContain("voiceActors(language: JAPANESE");
		GraphQlAssertions.AssertValidGraphQl(seyuWithoutAnime);
	}

	private static int OccurrenceCount(string haystack, string needle) => haystack.Split(needle, StringSplitOptions.None).Length - 1;

	private static Dictionary<string, object?> VariablesOf(global::GraphQL.GraphQLRequest request) => (Dictionary<string, object?>)request.Variables!;

	private static string DeclaredVariableNames(string query)
	{
		var open = query.IndexOf('(', StringComparison.Ordinal);
		var close = query.IndexOf(')', StringComparison.Ordinal);
		var declarations = query[(open + 1)..close].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		var names = declarations
			.Select(d => d.Split(':', StringSplitOptions.TrimEntries)[0].TrimStart('$'))
			.Order(StringComparer.Ordinal);
		return string.Join(',', names);
	}

	private static string SuppliedVariableNames(object? variables)
	{
		var source = variables ?? throw new ArgumentNullException(nameof(variables));
		var names = source is IReadOnlyDictionary<string, object?> dictionary
			? dictionary.Keys
			: source.GetType().GetProperties().Select(p => p.Name);
		return string.Join(',', names.Order(StringComparer.Ordinal));
	}
}
