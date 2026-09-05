// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using PaperMalKing.MyAnimeList.UpdateProvider.Search;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;
using PaperMalKing.UpdatesProviders.Base.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;

public sealed class MalMediaCandidateTests
{
	private const string Monster = "Monster";
	private const string MonsterStory = "Monster Story";
	private const string Kaibutsu = "Kaibutsu";
	private const string MonsterNative = "モンスター";
	private const int AnimeStartDateYear = 2003;
	private const int AnimeSeasonYear = 2004;
	private const int MangaStartYear = 1988;
	private const uint MonsterMembers = 1_400_000U;

	[Test]
	public async Task MatchTitlesCarryTheExpectedRanks()
	{
		var media = Anime(1U, Monster, synonyms: [MonsterStory], japanese: MonsterNative, english: Kaibutsu);

		var candidate = MalMediaCandidate.Create(media, mediaTypeFilter: null);

		await Assert.That(candidate.MatchTitles).Contains((Monster, MatchRank.Primary));
		await Assert.That(candidate.MatchTitles).Contains((MonsterStory, MatchRank.Synonym));
		await Assert.That(candidate.MatchTitles).Contains((MonsterNative, MatchRank.Native));
		await Assert.That(candidate.MatchTitles).Contains((Kaibutsu, MatchRank.English));
	}

	[Test]
	public async Task PrimaryTitleIsThePrimaryTitleField()
	{
		var media = Anime(1U, Monster);

		var candidate = MalMediaCandidate.Create(media, mediaTypeFilter: null);

		await Assert.That(candidate.PrimaryTitle).IsEqualTo(Monster);
	}

	[Test]
	public async Task PopularityIsTheListUserCount()
	{
		var media = Anime(1U, Monster, listUserCount: MonsterMembers);

		var candidate = MalMediaCandidate.Create(media, mediaTypeFilter: null);

		await Assert.That(candidate.Popularity).IsEqualTo(MonsterMembers);
	}

	[Test]
	public async Task PassesTypeFilterComparesTheAnimeMediaTypeToTheRequestedFilter()
	{
		var media = Anime(1U, Monster, mediaType: AnimeMediaType.TV);

		var matching = MalMediaCandidate.Create(media, AnimeMediaType.TV);
		var mismatched = MalMediaCandidate.Create(media, AnimeMediaType.Movie);
		var unfiltered = MalMediaCandidate.Create(media, mediaTypeFilter: null);

		await Assert.That(matching.PassesTypeFilter).IsTrue();
		await Assert.That(mismatched.PassesTypeFilter).IsFalse();
		await Assert.That(unfiltered.PassesTypeFilter).IsTrue();
	}

	[Test]
	public async Task PassesTypeFilterComparesTheMangaMediaTypeToTheRequestedFilter()
	{
		var media = Manga(1U, MangaMediaType.LightNovel);

		var matching = MalMediaCandidate.Create(media, MangaMediaType.LightNovel);
		var mismatched = MalMediaCandidate.Create(media, MangaMediaType.Manga);

		await Assert.That(matching.PassesTypeFilter).IsTrue();
		await Assert.That(mismatched.PassesTypeFilter).IsFalse();
	}

	[Test]
	public async Task OptionDescriptionComposesTypeYearScoreAndMembers()
	{
		var media = new AnimeSearchResult
		{
			Id = 42U,
			PrimaryTitle = Monster,
			MediaType = AnimeMediaType.TV,
			Status = AnimeAiringStatus.Unknown,
			Episodes = 0U,
			ListUserCount = MonsterMembers,
			Genres = [],
			Mean = 8.88,
			StartDate = new(AnimeSeasonYear, 1, 1),
		};

		var candidate = MalMediaCandidate.Create(media, mediaTypeFilter: null);

		await Assert.That(candidate.OptionDescription).IsEqualTo(
			$"TV · {AnimeSeasonYear.ToString(CultureInfo.InvariantCulture)} · ★ 8.88 · 1.4M members");
	}

	[Test]
	public async Task OptionDescriptionYearComesFromTheStartDate()
	{
		var media = Anime(
			1U,
			Monster,
			startDate: new(AnimeStartDateYear, 4, 7),
			startSeason: new() { Season = AnimeSeason.Spring, Year = AnimeSeasonYear });

		var candidate = MalMediaCandidate.Create(media, mediaTypeFilter: null);

		await Assert.That(candidate.OptionDescription).Contains(AnimeStartDateYear.ToString(CultureInfo.InvariantCulture));
	}

	[Test]
	public async Task OptionDescriptionForMangaComposesTypeYearAndMembers()
	{
		var media = Manga(1U, MangaMediaType.LightNovel, startDate: new(MangaStartYear, 9, 1));

		var candidate = MalMediaCandidate.Create(media, mediaTypeFilter: null);

		await Assert.That(candidate.OptionDescription).IsEqualTo(
			$"Light novel · {MangaStartYear.ToString(CultureInfo.InvariantCulture)} · 0 members");
	}

	private static AnimeSearchResult Anime(
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

	private static MangaSearchResult Manga(uint id, MangaMediaType mediaType, DateOnly? startDate = null) => new()
	{
		Id = id,
		PrimaryTitle = Monster,
		MediaType = mediaType,
		Status = MangaPublishingStatus.Unknown,
		Chapters = 0U,
		Volumes = 0U,
		ListUserCount = 0U,
		Genres = [],
		StartDate = startDate,
	};
}
