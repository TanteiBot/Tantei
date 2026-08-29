// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.MyAnimeList.UpdateProvider.Search;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;
using TUnit.Assertions.Enums;
using static PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search.SearchResultRankerTests;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;

public sealed class SearchPipelineTests
{
	private const string Monster = "Monster";
	private const uint PrimaryResultId = 2U;
	private const uint LowestSortedId = 20U;
	private const uint MiddleSortedId = 30U;
	private const uint HighestSortedId = 40U;
	private const uint ContainsSortedId = 10U;

	[Test]
	public async Task FloorRunsBeforeTheMediaTypeFilter()
	{
		var response = Response(
			Anime(1U, Monster, mediaType: AnimeMediaType.TV),
			Anime(PrimaryResultId, "Kaibutsu", mediaType: AnimeMediaType.Movie));

		var outcome = SearchPipeline.Evaluate(MatchKey.Create(Monster), response, AnimeMediaType.Movie);

		await Assert.That(outcome.Kind).IsEqualTo(SearchOutcomeKind.TypeFilterEmpty);
		await Assert.That(outcome.FloorSurvivorCount).IsEqualTo(1);
		await Assert.That(outcome.Results).IsEmpty();
	}

	[Test]
	public async Task EmptyMalResponseAndEmptyRelevanceFloorHaveTheSameOutcome()
	{
		var malEmpty = SearchPipeline.Evaluate(MatchKey.Create(Monster), AnimeSearchResponse.Empty, mediaTypeFilter: null);
		var floorEmpty = SearchPipeline.Evaluate(
			MatchKey.Create(Monster),
			Response(Anime(1U, "Kaibutsu", mediaType: AnimeMediaType.Movie)),
			AnimeMediaType.TV);

		await Assert.That(malEmpty.Kind).IsEqualTo(SearchOutcomeKind.NoResults);
		await Assert.That(floorEmpty.Kind).IsEqualTo(SearchOutcomeKind.NoResults);
		await Assert.That(floorEmpty.FloorSurvivorCount).IsEqualTo(0);
	}

	[Test]
	public async Task ResultsSortByRankThenMembersDescendingThenMalIdAscending()
	{
		var response = Response(
			Anime(HighestSortedId, Monster, listUserCount: 10U),
			Anime(MiddleSortedId, Monster, listUserCount: LowestSortedId),
			Anime(LowestSortedId, Monster, listUserCount: LowestSortedId),
			Anime(ContainsSortedId, "Pocket Monsters", listUserCount: 1_000U));

		var outcome = SearchPipeline.Evaluate(MatchKey.Create(Monster), response, mediaTypeFilter: null);

		await Assert.That(outcome.Results.Select(static ranked => ranked.Result.Id)).IsEquivalentTo(
			[LowestSortedId, MiddleSortedId, HighestSortedId, ContainsSortedId,],
			CollectionOrdering.Matching);
	}

	[Test]
	public async Task SolePrimaryTitleMatchAutoPostsFromLargerResultSet()
	{
		var response = Response(
			Anime(1U, "Pocket Monsters", listUserCount: 100U),
			Anime(PrimaryResultId, Monster, listUserCount: 1U),
			Anime(3U, "Monster Story", listUserCount: 200U, synonyms: [Monster]));

		var outcome = SearchPipeline.Evaluate(MatchKey.Create(Monster), response, mediaTypeFilter: null);

		await Assert.That(outcome.Kind).IsEqualTo(SearchOutcomeKind.AutoPost);
		await Assert.That(outcome.AutoPostResult).IsNotNull();
		await Assert.That(outcome.AutoPostResult!.Result.Id).IsEqualTo(PrimaryResultId);
	}

	[Test]
	public async Task OneNonPrimarySurvivorAutoPosts()
	{
		var response = Response(
			Anime(1U, "Boomba & Toomba"),
			Anime(PrimaryResultId, "Btooom!"));

		var outcome = SearchPipeline.Evaluate(MatchKey.Create("Toom"), response, mediaTypeFilter: null);

		await Assert.That(outcome.Kind).IsEqualTo(SearchOutcomeKind.AutoPost);
		await Assert.That(outcome.AutoPostResult).IsNotNull();
		await Assert.That(outcome.AutoPostResult!.Result.Id).IsEqualTo(1U);
	}

	[Test]
	public async Task DuplicatePrimaryTitleCollisionOpensPicker()
	{
		var response = Response(
			Anime(PrimaryResultId, Monster, listUserCount: 10U),
			Anime(1U, Monster, listUserCount: LowestSortedId));

		var outcome = SearchPipeline.Evaluate(MatchKey.Create(Monster), response, mediaTypeFilter: null);

		await Assert.That(outcome.Kind).IsEqualTo(SearchOutcomeKind.Picker);
		await Assert.That(outcome.AutoPostResult).IsNull();
	}

	[Test]
	public async Task MultipleResultsWithoutPrimaryTitleMatchOpenPicker()
	{
		var response = Response(
			Anime(1U, "Monster Story"),
			Anime(PrimaryResultId, "Pocket Monsters"));

		var outcome = SearchPipeline.Evaluate(MatchKey.Create(Monster), response, mediaTypeFilter: null);

		await Assert.That(outcome.Kind).IsEqualTo(SearchOutcomeKind.Picker);
		await Assert.That(outcome.AutoPostResult).IsNull();
	}

	private static AnimeSearchResponse Response(params AnimeSearchResult[] results) => new()
	{
		Results = [.. results.Select(static result => new SearchResultEnvelope<AnimeSearchResult> { Result = result, })],
	};
}
