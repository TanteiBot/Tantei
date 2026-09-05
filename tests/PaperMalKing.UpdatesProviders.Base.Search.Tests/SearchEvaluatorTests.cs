// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;
using TUnit.Assertions.Enums;

namespace PaperMalKing.UpdatesProviders.Base.Search.Tests;

public sealed class SearchEvaluatorTests
{
	private const string Monster = "Monster";
	private const string MonsterStory = "Monster Story";
	private const string PocketMonster = "Pocket Monster";
	private const string PocketMonsters = "Pocket Monsters";
	private const string Kaibutsu = "Kaibutsu";
	private const string MonsterNative = "モンスター";
	private const uint PrimaryResultId = 2U;
	private const uint ToomMatchId = 3U;
	private const uint DeletedBoundaryMatchId = 4U;
	private const uint IrrelevantResultId = 6U;
	private const uint LowestSortedId = 20U;
	private const uint MiddleSortedId = 30U;
	private const uint HighestSortedId = 40U;
	private const uint ContainsSortedId = 10U;
	private const long LowerPopularity = 10L;
	private const long HigherPopularity = 20L;

	[Test]
	public async Task EmptyQueryKeyIsRejected()
	{
		await Assert.That(() => SearchEvaluator.Evaluate(MatchKey.Create(""), [])).Throws<ArgumentException>();
	}

	[Test]
	public async Task NoRelevanceFloorSurvivorsYieldNoResults()
	{
		var empty = SearchEvaluator.Evaluate(MatchKey.Create(Monster), []);
		var allIrrelevant = SearchEvaluator.Evaluate(
			MatchKey.Create(Monster),
			[Candidate(1U, [(Kaibutsu, MatchRank.Primary)]), Candidate(PrimaryResultId, [("Naruto", MatchRank.Primary)])]);

		await Assert.That(empty.Kind).IsEqualTo(SearchOutcomeKind.NoResults);
		await Assert.That(empty.FloorSurvivorCount).IsEqualTo(0);
		await Assert.That(empty.Results).IsEmpty();
		await Assert.That(allIrrelevant.Kind).IsEqualTo(SearchOutcomeKind.NoResults);
		await Assert.That(allIrrelevant.FloorSurvivorCount).IsEqualTo(0);
	}

	[Test]
	public async Task BestRankWinsAcrossEveryTitleSourceAndIrrelevantResultsAreRemoved()
	{
		var candidates = new[]
		{
			Candidate(1U, [(Monster, MatchRank.Primary)]),
			Candidate(PrimaryResultId, [(PocketMonster, MatchRank.Primary), (Monster, MatchRank.Synonym)]),
			Candidate(ToomMatchId, [(MonsterStory, MatchRank.Primary), (PocketMonster, MatchRank.Synonym), (Monster, MatchRank.Native)]),
			Candidate(DeletedBoundaryMatchId, [(MonsterStory, MatchRank.Primary), (PocketMonster, MatchRank.Synonym), (Kaibutsu, MatchRank.Native), (Monster, MatchRank.English)]),
			Candidate(5U, [(PocketMonsters, MatchRank.Primary)]),
			Candidate(IrrelevantResultId, [(Kaibutsu, MatchRank.Primary)]),
		};

		var evaluation = SearchEvaluator.Evaluate(MatchKey.Create(Monster), candidates);

		await Assert.That(evaluation.Results.Select(static result => result.Rank)).IsEquivalentTo(
			[MatchRank.Primary, MatchRank.Synonym, MatchRank.Native, MatchRank.English, MatchRank.Contains,],
			CollectionOrdering.Matching);
		await Assert.That(evaluation.Results.Select(static result => result.Id)).DoesNotContain(IrrelevantResultId);
	}

	[Test]
	public async Task DuplicateKeysKeepTheBestTitleSourceAndEmptyKeysAreDiscarded()
	{
		var kon = new (string? Title, MatchRank Rank)[]
		{
			("K-On!", MatchRank.Primary),
			("K On", MatchRank.Synonym),
			("", MatchRank.Synonym),
			(null, MatchRank.Synonym),
			("!!!", MatchRank.Synonym),
			("Ｋ－ＯＮ！", MatchRank.Native),
			("K-On", MatchRank.English),
		};
		var primary = SearchEvaluator.Evaluate(MatchKey.Create("kon"), [Candidate(1U, kon)]);
		var synonym = SearchEvaluator.Evaluate(
			MatchKey.Create(Monster),
			[Candidate(PrimaryResultId, [("!!!", MatchRank.Primary), ("", MatchRank.Synonym), (null, MatchRank.Synonym), (Monster, MatchRank.Synonym)])]);

		await Assert.That(primary.Results.Single().Rank).IsEqualTo(MatchRank.Primary);
		await Assert.That(synonym.Results.Single().Rank).IsEqualTo(MatchRank.Synonym);
	}

	[Test]
	public async Task ContainmentUsesTheNormalizedOrdinalMatchKey()
	{
		var candidates = new[]
		{
			Candidate(1U, [("K-On!", MatchRank.Primary)]),
			Candidate(PrimaryResultId, [("Btooom!", MatchRank.Primary)]),
			Candidate(ToomMatchId, [("Boomba & Toomba", MatchRank.Primary)]),
			Candidate(DeletedBoundaryMatchId, [("Ao Haru Ride", MatchRank.Primary)]),
		};

		var kon = SearchEvaluator.Evaluate(MatchKey.Create("kon"), candidates);
		var toom = SearchEvaluator.Evaluate(MatchKey.Create("Toom"), candidates);
		var deletedBoundary = SearchEvaluator.Evaluate(MatchKey.Create("OHARU"), candidates);

		await Assert.That(kon.Results.Single().Id).IsEqualTo(1U);
		await Assert.That(kon.Results.Single().Rank).IsEqualTo(MatchRank.Primary);
		await Assert.That(toom.Results.Select(static result => result.Id)).IsEquivalentTo([ToomMatchId]);
		await Assert.That(toom.Results.Single().Rank).IsEqualTo(MatchRank.Contains);
		await Assert.That(deletedBoundary.Results.Select(static result => result.Id)).IsEquivalentTo([DeletedBoundaryMatchId]);
		await Assert.That(deletedBoundary.Results.Single().Rank).IsEqualTo(MatchRank.Contains);
	}

	[Test]
	public async Task ResultsSortByRankThenPopularityDescendingThenIdAscending()
	{
		var candidates = new[]
		{
			Candidate(HighestSortedId, [(Monster, MatchRank.Primary)], popularity: LowerPopularity),
			Candidate(MiddleSortedId, [(Monster, MatchRank.Primary)], popularity: HigherPopularity),
			Candidate(LowestSortedId, [(Monster, MatchRank.Primary)], popularity: HigherPopularity),
			Candidate(ContainsSortedId, [(PocketMonsters, MatchRank.Primary)], popularity: 1_000L),
		};

		var evaluation = SearchEvaluator.Evaluate(MatchKey.Create(Monster), candidates);

		await Assert.That(evaluation.Results.Select(static result => result.Id)).IsEquivalentTo(
			[LowestSortedId, MiddleSortedId, HighestSortedId, ContainsSortedId,],
			CollectionOrdering.Matching);
	}

	[Test]
	public async Task SolePrimaryTitleMatchAutoPostsFromLargerResultSet()
	{
		var candidates = new[]
		{
			Candidate(1U, [(PocketMonsters, MatchRank.Primary)], popularity: 100L),
			Candidate(PrimaryResultId, [(Monster, MatchRank.Primary)], popularity: 1L),
			Candidate(ToomMatchId, [(MonsterStory, MatchRank.Primary), (Monster, MatchRank.Synonym)], popularity: 200L),
		};

		var evaluation = SearchEvaluator.Evaluate(MatchKey.Create(Monster), candidates);

		await Assert.That(evaluation.Kind).IsEqualTo(SearchOutcomeKind.AutoPosted);
		await Assert.That(evaluation.AutoPostResult).IsNotNull();
		await Assert.That(evaluation.AutoPostResult!.Id).IsEqualTo(PrimaryResultId);
	}

	[Test]
	public async Task OneNonPrimarySurvivorAutoPosts()
	{
		var candidates = new[]
		{
			Candidate(1U, [("Boomba & Toomba", MatchRank.Primary)]),
			Candidate(PrimaryResultId, [("Btooom!", MatchRank.Primary)]),
		};

		var evaluation = SearchEvaluator.Evaluate(MatchKey.Create("Toom"), candidates);

		await Assert.That(evaluation.Kind).IsEqualTo(SearchOutcomeKind.AutoPosted);
		await Assert.That(evaluation.AutoPostResult).IsNotNull();
		await Assert.That(evaluation.AutoPostResult!.Id).IsEqualTo(1U);
	}

	[Test]
	public async Task DuplicatePrimaryTitleCollisionOpensPicker()
	{
		var candidates = new[]
		{
			Candidate(PrimaryResultId, [(Monster, MatchRank.Primary)], popularity: LowerPopularity),
			Candidate(1U, [(Monster, MatchRank.Primary)], popularity: HigherPopularity),
		};

		var evaluation = SearchEvaluator.Evaluate(MatchKey.Create(Monster), candidates);

		await Assert.That(evaluation.Kind).IsEqualTo(SearchOutcomeKind.PickerOpened);
		await Assert.That(evaluation.AutoPostResult).IsNull();
	}

	[Test]
	public async Task MultipleResultsWithoutPrimaryTitleMatchOpenPicker()
	{
		var candidates = new[]
		{
			Candidate(1U, [(MonsterStory, MatchRank.Primary)]),
			Candidate(PrimaryResultId, [(PocketMonsters, MatchRank.Primary)]),
		};

		var evaluation = SearchEvaluator.Evaluate(MatchKey.Create(Monster), candidates);

		await Assert.That(evaluation.Kind).IsEqualTo(SearchOutcomeKind.PickerOpened);
		await Assert.That(evaluation.AutoPostResult).IsNull();
	}

	[Test]
	public async Task MatchKeyNormalizationFoldsMacronsAndMatchesNativeScript()
	{
		var macron = SearchEvaluator.Evaluate(MatchKey.Create("Tokyo Ghoul"), [Candidate(1U, [("Tōkyō Ghoul", MatchRank.Primary)])]);
		var native = SearchEvaluator.Evaluate(MatchKey.Create(MonsterNative), [Candidate(PrimaryResultId, [(Monster, MatchRank.Primary), (MonsterNative, MatchRank.Native)])]);

		await Assert.That(macron.Kind).IsEqualTo(SearchOutcomeKind.AutoPosted);
		await Assert.That(macron.Results.Single().Rank).IsEqualTo(MatchRank.Primary);
		await Assert.That(native.Kind).IsEqualTo(SearchOutcomeKind.AutoPosted);
		await Assert.That(native.Results.Single().Rank).IsEqualTo(MatchRank.Native);
	}

	[Test]
	public async Task TypeFilterRunsAfterTheFloorAndEmptiesToTypeFilterEmpty()
	{
		var candidates = new[]
		{
			Candidate(1U, [(Monster, MatchRank.Primary)], passesTypeFilter: false),
			Candidate(PrimaryResultId, [(Kaibutsu, MatchRank.Primary)], passesTypeFilter: true),
		};

		var evaluation = SearchEvaluator.Evaluate(MatchKey.Create(Monster), candidates, applyTypeFilter: true);

		await Assert.That(evaluation.Kind).IsEqualTo(SearchOutcomeKind.TypeFilterEmpty);
		await Assert.That(evaluation.FloorSurvivorCount).IsEqualTo(1);
		await Assert.That(evaluation.Results).IsEmpty();
	}

	[Test]
	public async Task TypeFilterNeverEmptiesWhenNoFloorSurvivorsExist()
	{
		var evaluation = SearchEvaluator.Evaluate(
			MatchKey.Create(Monster),
			[Candidate(1U, [(Kaibutsu, MatchRank.Primary)], passesTypeFilter: false)],
			applyTypeFilter: true);

		await Assert.That(evaluation.Kind).IsEqualTo(SearchOutcomeKind.NoResults);
		await Assert.That(evaluation.FloorSurvivorCount).IsEqualTo(0);
	}

	[Test]
	public async Task ExcludedTypeFilterCandidatesAreDroppedButSurvivorsRemain()
	{
		var candidates = new[]
		{
			Candidate(1U, [(Monster, MatchRank.Primary)], passesTypeFilter: false),
			Candidate(PrimaryResultId, [(Monster, MatchRank.Primary)], passesTypeFilter: true),
		};

		var evaluation = SearchEvaluator.Evaluate(MatchKey.Create(Monster), candidates, applyTypeFilter: true);

		await Assert.That(evaluation.Kind).IsEqualTo(SearchOutcomeKind.AutoPosted);
		await Assert.That(evaluation.AutoPostResult!.Id).IsEqualTo(PrimaryResultId);
	}

	[Test]
	public async Task AutoPostResultCarriesTheProviderBuiltOptionDescriptionAndEmbed()
	{
		const string optionDescription = "TV · 2004 · 85/100 · 1.4M";
		var candidates = new[]
		{
			Candidate(
				PrimaryResultId,
				[(Monster, MatchRank.Primary)],
				optionDescription: optionDescription,
				buildEmbed: static context => new DiscordEmbedBuilder().WithTitle(context.Query)),
		};

		var evaluation = SearchEvaluator.Evaluate(MatchKey.Create(Monster), candidates);
		var result = evaluation.AutoPostResult!;
		var embed = result.BuildEmbed(new(
			Monster,
			PickerMediaKind.Anime,
			null,
			1UL,
			"Requester",
			null,
			2UL,
			3UL,
			DateTimeOffset.UnixEpoch));

		await Assert.That(evaluation.Kind).IsEqualTo(SearchOutcomeKind.AutoPosted);
		await Assert.That(result.Id).IsEqualTo(PrimaryResultId);
		await Assert.That(result.PrimaryTitle).IsEqualTo(Monster);
		await Assert.That(result.OptionDescription).IsEqualTo(optionDescription);
		await Assert.That(embed.Title).IsEqualTo(Monster);
	}

	private static SearchCandidate Candidate(
		uint id,
		(string? Title, MatchRank Rank)[] matchTitles,
		long popularity = 0L,
		string optionDescription = "",
		bool passesTypeFilter = true,
		Func<PickerSearchContext, DiscordEmbedBuilder>? buildEmbed = null) => new(
		id,
		popularity,
		matchTitles.Length == 0 ? "" : matchTitles[0].Title ?? "",
		matchTitles,
		optionDescription,
		buildEmbed ?? (static _ => new DiscordEmbedBuilder()),
		passesTypeFilter);
}
