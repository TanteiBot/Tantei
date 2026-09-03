// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;
using PaperMalKing.AniList.UpdateProvider.Search;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.UpdatesProviders.Base.Search;
using TUnit.Assertions.Enums;

namespace PaperMalKing.AniList.UpdateProvider.Tests.Search;

public sealed class SearchEvaluationTests
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
	private const int LowerPopularity = 10;
	private const int HigherPopularity = 20;

	[Test]
	public async Task NoRelevanceFloorSurvivorsYieldNoResults()
	{
		var empty = AniListMediaEvaluator.Evaluate(MatchKey.Create(Monster), TitleLanguage.Romaji, []);
		var allIrrelevant = AniListMediaEvaluator.Evaluate(
			MatchKey.Create(Monster),
			TitleLanguage.Romaji,
			[Candidate(1U, romaji: Kaibutsu), Candidate(PrimaryResultId, romaji: "Naruto", native: MonsterNative)]);

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
			Candidate(1U, romaji: Monster),
			Candidate(PrimaryResultId, romaji: PocketMonster, synonyms: [Monster]),
			Candidate(ToomMatchId, romaji: MonsterStory, native: Monster, synonyms: [PocketMonster]),
			Candidate(DeletedBoundaryMatchId, romaji: MonsterStory, english: Monster, native: Kaibutsu, synonyms: [PocketMonster]),
			Candidate(5U, romaji: PocketMonsters),
			Candidate(IrrelevantResultId, romaji: Kaibutsu),
		};

		var evaluation = AniListMediaEvaluator.Evaluate(MatchKey.Create(Monster), TitleLanguage.Romaji, candidates);

		await Assert.That(evaluation.Results.Select(static result => result.Rank)).IsEquivalentTo(
			[MatchRank.Primary, MatchRank.Synonym, MatchRank.Native, MatchRank.English, MatchRank.Contains,],
			CollectionOrdering.Matching);
		await Assert.That(evaluation.Results.Select(static result => result.Id)).DoesNotContain(IrrelevantResultId);
	}

	[Test]
	public async Task RomajiTitleIsAlwaysPrimaryEvenForAnEnglishPreferringRequester()
	{
		var candidates = new[] { Candidate(1U, romaji: Monster, english: Kaibutsu), };

		var evaluation = AniListMediaEvaluator.Evaluate(MatchKey.Create(Monster), TitleLanguage.English, candidates);

		await Assert.That(evaluation.Kind).IsEqualTo(SearchOutcomeKind.AutoPosted);
		await Assert.That(evaluation.Results.Single().Rank).IsEqualTo(MatchRank.Primary);
		await Assert.That(evaluation.AutoPostResult!.PrimaryTitle).IsEqualTo(Kaibutsu);
	}

	[Test]
	public async Task RequesterResolvedTitleIsPrimary()
	{
		var candidates = new[] { Candidate(1U, romaji: PocketMonster, english: Monster), };

		var evaluation = AniListMediaEvaluator.Evaluate(MatchKey.Create(Monster), TitleLanguage.English, candidates);

		await Assert.That(evaluation.Kind).IsEqualTo(SearchOutcomeKind.AutoPosted);
		await Assert.That(evaluation.Results.Single().Rank).IsEqualTo(MatchRank.Primary);
	}

	[Test]
	public async Task DuplicateKeysKeepTheBestTitleSourceAndEmptyKeysAreDiscarded()
	{
		var candidates = new[]
		{
			Candidate(1U, romaji: "K-On!", english: "K-On", native: "Ｋ－ＯＮ！", synonyms: ["K On", "", null, "!!!"]),
			Candidate(PrimaryResultId, romaji: "!!!", synonyms: ["", null, Monster]),
			Candidate(ToomMatchId, romaji: Monster, english: Monster),
		};

		var primary = AniListMediaEvaluator.Evaluate(MatchKey.Create("kon"), TitleLanguage.Romaji, candidates);
		var synonym = AniListMediaEvaluator.Evaluate(MatchKey.Create(Monster), TitleLanguage.Romaji, [candidates[1]]);
		var romajiPrimary = AniListMediaEvaluator.Evaluate(MatchKey.Create(Monster), TitleLanguage.Romaji, [candidates[2]]);

		await Assert.That(primary.Results.Single().Rank).IsEqualTo(MatchRank.Primary);
		await Assert.That(synonym.Results.Single().Rank).IsEqualTo(MatchRank.Synonym);
		await Assert.That(romajiPrimary.Results.Single().Rank).IsEqualTo(MatchRank.Primary);
	}

	[Test]
	public async Task ContainmentUsesTheNormalizedOrdinalMatchKey()
	{
		var candidates = new[]
		{
			Candidate(1U, romaji: "K-On!"),
			Candidate(PrimaryResultId, romaji: "Btooom!"),
			Candidate(ToomMatchId, romaji: "Boomba & Toomba"),
			Candidate(DeletedBoundaryMatchId, romaji: "Ao Haru Ride"),
		};

		var kon = AniListMediaEvaluator.Evaluate(MatchKey.Create("kon"), TitleLanguage.Romaji, candidates);
		var toom = AniListMediaEvaluator.Evaluate(MatchKey.Create("Toom"), TitleLanguage.Romaji, candidates);
		var deletedBoundary = AniListMediaEvaluator.Evaluate(MatchKey.Create("OHARU"), TitleLanguage.Romaji, candidates);

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
			Candidate(HighestSortedId, romaji: Monster, popularity: LowerPopularity),
			Candidate(MiddleSortedId, romaji: Monster, popularity: HigherPopularity),
			Candidate(LowestSortedId, romaji: Monster, popularity: HigherPopularity),
			Candidate(ContainsSortedId, romaji: PocketMonsters, popularity: 1_000),
		};

		var evaluation = AniListMediaEvaluator.Evaluate(MatchKey.Create(Monster), TitleLanguage.Romaji, candidates);

		await Assert.That(evaluation.Results.Select(static result => result.Id)).IsEquivalentTo(
			[LowestSortedId, MiddleSortedId, HighestSortedId, ContainsSortedId,],
			CollectionOrdering.Matching);
	}

	[Test]
	public async Task SolePrimaryTitleMatchAutoPostsFromLargerResultSet()
	{
		var candidates = new[]
		{
			Candidate(1U, romaji: PocketMonsters, popularity: 100),
			Candidate(PrimaryResultId, romaji: Monster, popularity: 1),
			Candidate(3U, romaji: MonsterStory, synonyms: [Monster], popularity: 200),
		};

		var evaluation = AniListMediaEvaluator.Evaluate(MatchKey.Create(Monster), TitleLanguage.Romaji, candidates);

		await Assert.That(evaluation.Kind).IsEqualTo(SearchOutcomeKind.AutoPosted);
		await Assert.That(evaluation.AutoPostResult).IsNotNull();
		await Assert.That(evaluation.AutoPostResult!.Id).IsEqualTo(PrimaryResultId);
	}

	[Test]
	public async Task OneNonPrimarySurvivorAutoPosts()
	{
		var candidates = new[]
		{
			Candidate(1U, romaji: "Boomba & Toomba"),
			Candidate(PrimaryResultId, romaji: "Btooom!"),
		};

		var evaluation = AniListMediaEvaluator.Evaluate(MatchKey.Create("Toom"), TitleLanguage.Romaji, candidates);

		await Assert.That(evaluation.Kind).IsEqualTo(SearchOutcomeKind.AutoPosted);
		await Assert.That(evaluation.AutoPostResult).IsNotNull();
		await Assert.That(evaluation.AutoPostResult!.Id).IsEqualTo(1U);
	}

	[Test]
	public async Task DuplicatePrimaryTitleCollisionOpensPicker()
	{
		var candidates = new[]
		{
			Candidate(PrimaryResultId, romaji: Monster, popularity: LowerPopularity),
			Candidate(1U, romaji: Monster, popularity: HigherPopularity),
		};

		var evaluation = AniListMediaEvaluator.Evaluate(MatchKey.Create(Monster), TitleLanguage.Romaji, candidates);

		await Assert.That(evaluation.Kind).IsEqualTo(SearchOutcomeKind.PickerOpened);
		await Assert.That(evaluation.AutoPostResult).IsNull();
	}

	[Test]
	public async Task MultipleResultsWithoutPrimaryTitleMatchOpenPicker()
	{
		var candidates = new[]
		{
			Candidate(1U, romaji: MonsterStory),
			Candidate(PrimaryResultId, romaji: PocketMonsters),
		};

		var evaluation = AniListMediaEvaluator.Evaluate(MatchKey.Create(Monster), TitleLanguage.Romaji, candidates);

		await Assert.That(evaluation.Kind).IsEqualTo(SearchOutcomeKind.PickerOpened);
		await Assert.That(evaluation.AutoPostResult).IsNull();
	}

	[Test]
	public async Task MatchKeyNormalizationFoldsRomajiMacrons()
	{
		var candidates = new[] { Candidate(1U, romaji: "Tōkyō Ghoul"), };

		var evaluation = AniListMediaEvaluator.Evaluate(MatchKey.Create("Tokyo Ghoul"), TitleLanguage.Romaji, candidates);

		await Assert.That(evaluation.Kind).IsEqualTo(SearchOutcomeKind.AutoPosted);
		await Assert.That(evaluation.Results.Single().Rank).IsEqualTo(MatchRank.Primary);
	}

	[Test]
	public async Task MatchKeyNormalizationMatchesNativeScriptTitles()
	{
		var candidates = new[] { Candidate(1U, romaji: Monster, native: MonsterNative), };

		var evaluation = AniListMediaEvaluator.Evaluate(MatchKey.Create(MonsterNative), TitleLanguage.Native, candidates);

		await Assert.That(evaluation.Kind).IsEqualTo(SearchOutcomeKind.AutoPosted);
		await Assert.That(evaluation.Results.Single().Rank).IsEqualTo(MatchRank.Primary);
		await Assert.That(evaluation.AutoPostResult!.PrimaryTitle).IsEqualTo(MonsterNative);
	}

	[Test]
	public async Task AutoPostResultCarriesTheProviderBuiltOptionDescriptionAndEmbed()
	{
		const string optionDescription = "TV · 2004 · 85/100 · 1.4M";
		var candidates = new[]
		{
			Candidate(
				PrimaryResultId,
				romaji: Monster,
				optionDescription: optionDescription,
				buildEmbed: context => new DiscordEmbedBuilder().WithTitle(context.Query)),
		};

		var evaluation = AniListMediaEvaluator.Evaluate(MatchKey.Create(Monster), TitleLanguage.Romaji, candidates);
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
		await Assert.That(result.OptionDescription).IsEqualTo(optionDescription);
		await Assert.That(embed.Title).IsEqualTo(Monster);
	}

	private static AniListMediaCandidate Candidate(
		uint id,
		string? romaji = null,
		string? english = null,
		string? native = null,
		IReadOnlyList<string?>? synonyms = null,
		int popularity = 0,
		string optionDescription = "",
		Func<PickerSearchContext, DiscordEmbedBuilder>? buildEmbed = null) => new(
		id,
		new MediaTitle
		{
			Romaji = romaji,
			English = english,
			Native = native,
		},
		synonyms ?? [],
		popularity,
		optionDescription,
		buildEmbed ?? (static _ => new DiscordEmbedBuilder()));
}
