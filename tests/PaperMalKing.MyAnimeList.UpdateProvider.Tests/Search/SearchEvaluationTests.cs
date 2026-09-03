// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using PaperMalKing.MyAnimeList.UpdateProvider.Search;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;
using PaperMalKing.UpdatesProviders.Base.Search;
using TUnit.Assertions.Enums;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;

public sealed class SearchEvaluationTests
{
	private const string Monster = "Monster";
	private const string MonsterStory = "Monster Story";
	private const string PocketMonsters = "Pocket Monsters";
	private const string Kaibutsu = "Kaibutsu";
	private const uint PrimaryResultId = 2U;
	private const uint ToomMatchId = 3U;
	private const uint DeletedBoundaryMatchId = 4U;
	private const uint IrrelevantResultId = 6U;
	private const int ExpectedAnimeStartDateYear = 2003;
	private const int ExpectedAnimeStartSeasonYear = 2004;
	private const int ExpectedMangaStartYear = 1988;
	private const int ExpectedMangaStartMonth = 9;

	[Test]
	public async Task FloorRunsBeforeTheMediaTypeFilter()
	{
		var response = Response(
			Anime(1U, Monster, mediaType: AnimeMediaType.TV),
			Anime(PrimaryResultId, Kaibutsu, mediaType: AnimeMediaType.Movie));

		var evaluation = Evaluate(MatchKey.Create(Monster), response, AnimeMediaType.Movie);

		await Assert.That(evaluation.Kind).IsEqualTo(SearchOutcomeKind.TypeFilterEmpty);
		await Assert.That(evaluation.FloorSurvivorCount).IsEqualTo(1);
		await Assert.That(evaluation.Results).IsEmpty();
	}

	[Test]
	public async Task EmptyMalResponseAndEmptyRelevanceFloorHaveTheSameOutcome()
	{
		var malEmpty = Evaluate(MatchKey.Create(Monster), Response(), mediaTypeFilter: null);
		var floorEmpty = Evaluate(
			MatchKey.Create(Monster),
			Response(Anime(1U, Kaibutsu, mediaType: AnimeMediaType.Movie)),
			AnimeMediaType.TV);

		await Assert.That(malEmpty.Kind).IsEqualTo(SearchOutcomeKind.NoResults);
		await Assert.That(floorEmpty.Kind).IsEqualTo(SearchOutcomeKind.NoResults);
		await Assert.That(floorEmpty.FloorSurvivorCount).IsEqualTo(0);
	}

	[Test]
	public async Task CandidateExtractionMapsEachTitleSourceToItsRank()
	{
		var response = Response(
			Anime(1U, Monster, synonyms: [MonsterStory]),
			Anime(2U, "Pocket Monster", synonyms: [Monster]),
			Anime(ToomMatchId, MonsterStory, synonyms: ["Pocket Monster"], japanese: Monster),
			Anime(DeletedBoundaryMatchId, MonsterStory, synonyms: ["Pocket Monster"], japanese: Kaibutsu, english: Monster),
			Anime(5U, PocketMonsters),
			Anime(IrrelevantResultId, Kaibutsu));

		var evaluation = Evaluate(MatchKey.Create(Monster), response, mediaTypeFilter: null);

		await Assert.That(evaluation.Results.Select(static result => result.Rank)).IsEquivalentTo(
			[MatchRank.Primary, MatchRank.Synonym, MatchRank.Native, MatchRank.English, MatchRank.Contains,],
			CollectionOrdering.Matching);
		await Assert.That(evaluation.Results.Select(static result => result.Id)).DoesNotContain(IrrelevantResultId);
	}

	[Test]
	public async Task PickerAnimeYearComesFromTheStartDate()
	{
		var results = Response(Anime(
			1U,
			Monster,
			startDate: new(ExpectedAnimeStartDateYear, 4, 7),
			startSeason: new() { Season = AnimeSeason.Spring, Year = ExpectedAnimeStartSeasonYear, }));

		var evaluation = Evaluate(MatchKey.Create(Monster), results, mediaTypeFilter: null);

		await Assert.That(evaluation.Results.Single().OptionDescription).Contains(ExpectedAnimeStartDateYear.ToString(CultureInfo.InvariantCulture));
	}

	[Test]
	public async Task PickerOptionDescriptionComposesTypeYearScoreAndMembers()
	{
		var response = Response(new AnimeSearchResult
		{
			Id = 42U,
			PrimaryTitle = Monster,
			MediaType = AnimeMediaType.TV,
			Status = AnimeAiringStatus.Unknown,
			Episodes = 0U,
			ListUserCount = 1_400_000U,
			Genres = [],
			Mean = 8.88,
			StartDate = new(ExpectedAnimeStartSeasonYear, 1, 1),
		});

		var evaluation = Evaluate(MatchKey.Create(Monster), response, mediaTypeFilter: null);

		await Assert.That(evaluation.Results.Single().OptionDescription).IsEqualTo(
			$"TV · {ExpectedAnimeStartSeasonYear.ToString(CultureInfo.InvariantCulture)} · ★ 8.88 · 1.4M members");
	}

	[Test]
	public async Task MangaUsesTheSameRulesAndProjectsThePostingAndPickerResult()
	{
		var results = new[]
		{
			Manga(1U, MangaMediaType.Manga),
			Manga(PrimaryResultId, MangaMediaType.LightNovel),
		};

		var evaluation = Evaluate<MangaMediaType, MangaPublishingStatus>(
			MatchKey.Create(Monster),
			results,
			MangaMediaType.LightNovel);
		var result = evaluation.AutoPostResult!;
		var embed = result.BuildEmbed(new(
			Monster,
			PickerMediaKind.Manga,
			nameof(MangaMediaType.LightNovel),
			1UL,
			"Requester",
			null,
			2UL,
			3UL,
			DateTimeOffset.UnixEpoch));

		await Assert.That(evaluation.Kind).IsEqualTo(SearchOutcomeKind.AutoPosted);
		await Assert.That(result.Id).IsEqualTo(PrimaryResultId);
		await Assert.That(result.OptionDescription).IsEqualTo(
			$"Light novel · {ExpectedMangaStartYear.ToString(CultureInfo.InvariantCulture)} · 0 members");
		await Assert.That(embed.Title).IsEqualTo(Monster);
	}

	private static SearchEvaluation Evaluate<TMediaType, TStatus>(
		MatchKey queryKey,
		IEnumerable<BaseSearchResult<TMediaType, TStatus>> results,
		TMediaType? mediaTypeFilter)
		where TMediaType : unmanaged, Enum
		where TStatus : unmanaged, Enum => SearchEvaluator.Evaluate(
		queryKey,
		results.Select(result => MalMediaCandidate.Create(result, mediaTypeFilter)),
		applyTypeFilter: mediaTypeFilter.HasValue);

	internal static AnimeSearchResult Anime(
		uint id,
		string title,
		uint listUserCount = 0U,
		AnimeMediaType mediaType = AnimeMediaType.TV,
		IReadOnlyList<string?>? synonyms = null,
		string? japanese = null,
		string? english = null,
		DateOnly? startDate = null,
		AnimeStartSeason? startSeason = null) => new()
		{
			Id = id,
			PrimaryTitle = title,
			MediaType = mediaType,
			Status = AnimeAiringStatus.Unknown,
			Episodes = 0U,
			ListUserCount = listUserCount,
			Genres = [],
			StartDate = startDate,
			StartSeason = startSeason,
			AlternativeTitles = synonyms is null && japanese is null && english is null
				? null
				: new()
				{
					Synonyms = synonyms,
					Japanese = japanese,
					English = english,
				},
		};

	private static AnimeSearchResult[] Response(params AnimeSearchResult[] results) => results;

	private static MangaSearchResult Manga(uint id, MangaMediaType mediaType) => new()
	{
		Id = id,
		PrimaryTitle = Monster,
		MediaType = mediaType,
		Status = MangaPublishingStatus.Unknown,
		Chapters = 0U,
		Volumes = 0U,
		ListUserCount = 0U,
		Genres = [],
		StartDate = id == PrimaryResultId ? new(ExpectedMangaStartYear, ExpectedMangaStartMonth, 1) : null,
	};
}
