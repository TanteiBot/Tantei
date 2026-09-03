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

	[Test]
	public async Task CandidateExtractionMapsEachTitleSourceToItsRank()
	{
		var candidates = new[]
		{
			Candidate(1U, TitleLanguage.Romaji, romaji: Monster),
			Candidate(PrimaryResultId, TitleLanguage.Romaji, romaji: PocketMonster, synonyms: [Monster]),
			Candidate(ToomMatchId, TitleLanguage.Romaji, romaji: MonsterStory, native: Monster, synonyms: [PocketMonster]),
			Candidate(DeletedBoundaryMatchId, TitleLanguage.Romaji, romaji: MonsterStory, english: Monster, native: Kaibutsu, synonyms: [PocketMonster]),
			Candidate(5U, TitleLanguage.Romaji, romaji: PocketMonsters),
			Candidate(IrrelevantResultId, TitleLanguage.Romaji, romaji: Kaibutsu),
		};

		var evaluation = SearchEvaluator.Evaluate(MatchKey.Create(Monster), candidates);

		await Assert.That(evaluation.Results.Select(static result => result.Rank)).IsEquivalentTo(
			[MatchRank.Primary, MatchRank.Synonym, MatchRank.Native, MatchRank.English, MatchRank.Contains,],
			CollectionOrdering.Matching);
		await Assert.That(evaluation.Results.Select(static result => result.Id)).DoesNotContain(IrrelevantResultId);
	}

	[Test]
	public async Task RomajiTitleIsAlwaysPrimaryEvenForAnEnglishPreferringRequester()
	{
		var candidates = new[] { Candidate(1U, TitleLanguage.English, romaji: Monster, english: Kaibutsu), };

		var evaluation = SearchEvaluator.Evaluate(MatchKey.Create(Monster), candidates);

		await Assert.That(evaluation.Kind).IsEqualTo(SearchOutcomeKind.AutoPosted);
		await Assert.That(evaluation.Results.Single().Rank).IsEqualTo(MatchRank.Primary);
		await Assert.That(evaluation.AutoPostResult!.PrimaryTitle).IsEqualTo(Kaibutsu);
	}

	[Test]
	public async Task RequesterResolvedTitleIsPrimary()
	{
		var candidates = new[] { Candidate(1U, TitleLanguage.English, romaji: PocketMonster, english: Monster), };

		var evaluation = SearchEvaluator.Evaluate(MatchKey.Create(Monster), candidates);

		await Assert.That(evaluation.Kind).IsEqualTo(SearchOutcomeKind.AutoPosted);
		await Assert.That(evaluation.Results.Single().Rank).IsEqualTo(MatchRank.Primary);
	}

	[Test]
	public async Task ResolvedTitleAndRomajiBothCountAsPrimaryAfterDeduplication()
	{
		var synonymPrimary = SearchEvaluator.Evaluate(
			MatchKey.Create(Monster),
			[Candidate(PrimaryResultId, TitleLanguage.Romaji, romaji: "!!!", synonyms: ["", null, Monster])]);
		var romajiPrimary = SearchEvaluator.Evaluate(
			MatchKey.Create(Monster),
			[Candidate(ToomMatchId, TitleLanguage.Romaji, romaji: Monster, english: Monster)]);

		await Assert.That(synonymPrimary.Results.Single().Rank).IsEqualTo(MatchRank.Synonym);
		await Assert.That(romajiPrimary.Results.Single().Rank).IsEqualTo(MatchRank.Primary);
	}

	[Test]
	public async Task NativeScriptTitleResolvesAsThePrimaryTitle()
	{
		var candidates = new[] { Candidate(1U, TitleLanguage.Native, romaji: Monster, native: MonsterNative), };

		var evaluation = SearchEvaluator.Evaluate(MatchKey.Create(MonsterNative), candidates);

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
				TitleLanguage.Romaji,
				romaji: Monster,
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
		await Assert.That(result.OptionDescription).IsEqualTo(optionDescription);
		await Assert.That(embed.Title).IsEqualTo(Monster);
	}

	private static SearchCandidate Candidate(
		uint id,
		TitleLanguage titleLanguage,
		string? romaji = null,
		string? english = null,
		string? native = null,
		IReadOnlyList<string?>? synonyms = null,
		long popularity = 0L,
		string optionDescription = "",
		Func<PickerSearchContext, DiscordEmbedBuilder>? buildEmbed = null) => AniListMediaCandidate.Create(
		id,
		new MediaTitle
		{
			Romaji = romaji,
			English = english,
			Native = native,
		},
		synonyms ?? [],
		popularity,
		titleLanguage,
		optionDescription,
		buildEmbed ?? (static _ => new DiscordEmbedBuilder()));
}
