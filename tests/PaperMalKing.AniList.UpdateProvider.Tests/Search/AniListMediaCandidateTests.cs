// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;
using PaperMalKing.AniList.UpdateProvider.Search;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.UpdatesProviders.Base.Search;

namespace PaperMalKing.AniList.UpdateProvider.Tests.Search;

public sealed class AniListMediaCandidateTests
{
	private const ushort SeasonYear = 2004;
	private const ushort AverageScore = 85;
	private const uint Popularity = 1_400_000;
	private const uint SmallPopularity = 999;
	private const string Monster = "Monster";
	private const string MonsterStory = "Monster Story";
	private const string Kaibutsu = "Kaibutsu";
	private const string MonsterNative = "モンスター";

	[Test]
	public async Task OptionDescriptionComposesFormatYearScoreAndPopularity()
	{
		var media = Media(format: MediaFormat.Movie, seasonYear: SeasonYear, averageScore: AverageScore, popularity: Popularity);

		await Assert.That(OptionDescription(media, ScoreFormat.POINT_100)).IsEqualTo("Movie · 2004 · 85/100 · 1.4M");
	}

	[Test]
	public async Task OptionDescriptionRendersScoreInTheRequesterScoreFormat()
	{
		var media = Media(format: MediaFormat.Movie, seasonYear: SeasonYear, averageScore: AverageScore, popularity: Popularity);

		await Assert.That(OptionDescription(media, ScoreFormat.POINT_10_DECIMAL)).IsEqualTo("Movie · 2004 · 8.5/10 · 1.4M");
	}

	[Test]
	public async Task OptionDescriptionForAdultResultsCarriesTheBadge()
	{
		var media = Media(format: MediaFormat.Movie, seasonYear: SeasonYear, averageScore: AverageScore, popularity: Popularity, isAdult: true);

		await Assert.That(OptionDescription(media, ScoreFormat.POINT_100)).IsEqualTo("Movie · 2004 · 85/100 · 1.4M · 🔞");
	}

	[Test]
	public async Task OptionDescriptionOmitsTheNullScoreToken()
	{
		var media = Media(format: MediaFormat.Movie, seasonYear: SeasonYear, averageScore: null, popularity: Popularity);

		await Assert.That(OptionDescription(media, ScoreFormat.POINT_100)).IsEqualTo("Movie · 2004 · 1.4M");
	}

	[Test]
	public async Task OptionDescriptionOmitsNullFormatAndYear()
	{
		var media = Media(format: null, seasonYear: null, averageScore: AverageScore, popularity: SmallPopularity);

		await Assert.That(OptionDescription(media, ScoreFormat.POINT_100)).IsEqualTo("85/100 · 999");
	}

	[Test]
	public async Task MatchTitlesMapEachTitleSourceToItsRank()
	{
		var media = TitledMedia(romaji: Monster, english: Kaibutsu, native: MonsterNative, synonyms: [MonsterStory]);

		var candidate = AniListMediaCandidate.Create(media, TitleLanguage.Romaji, ScoreFormat.POINT_100, static _ => new DiscordEmbedBuilder());

		await Assert.That(candidate.MatchTitles).Contains((Monster, MatchRank.Primary));
		await Assert.That(candidate.MatchTitles).Contains((MonsterStory, MatchRank.Synonym));
		await Assert.That(candidate.MatchTitles).Contains((MonsterNative, MatchRank.Native));
		await Assert.That(candidate.MatchTitles).Contains((Kaibutsu, MatchRank.English));
	}

	[Test]
	public async Task RomajiAndTheResolvedTitleAreBothPrimary()
	{
		var media = TitledMedia(romaji: Monster, english: Kaibutsu);

		var candidate = AniListMediaCandidate.Create(media, TitleLanguage.English, ScoreFormat.POINT_100, static _ => new DiscordEmbedBuilder());

		var primaries = candidate.MatchTitles.Where(static title => title.Rank == MatchRank.Primary).Select(static title => title.Title);
		await Assert.That(primaries).Contains(Monster);
		await Assert.That(primaries).Contains(Kaibutsu);
	}

	[Test]
	public async Task PrimaryTitleResolvesForAnEnglishPreferringRequester()
	{
		var media = TitledMedia(romaji: Monster, english: Kaibutsu);

		var candidate = AniListMediaCandidate.Create(media, TitleLanguage.English, ScoreFormat.POINT_100, static _ => new DiscordEmbedBuilder());

		await Assert.That(candidate.PrimaryTitle).IsEqualTo(Kaibutsu);
	}

	[Test]
	public async Task PrimaryTitleResolvesToTheNativeScript()
	{
		var media = TitledMedia(romaji: Monster, native: MonsterNative);

		var candidate = AniListMediaCandidate.Create(media, TitleLanguage.Native, ScoreFormat.POINT_100, static _ => new DiscordEmbedBuilder());

		await Assert.That(candidate.PrimaryTitle).IsEqualTo(MonsterNative);
	}

	private static string OptionDescription(SearchMedia media, ScoreFormat scoreFormat) =>
		AniListMediaCandidate.Create(media, TitleLanguage.Romaji, scoreFormat, static _ => new DiscordEmbedBuilder()).OptionDescription;

	private static SearchMedia Media(
		MediaFormat? format,
		ushort? seasonYear,
		ushort? averageScore,
		uint popularity,
		bool isAdult = false) => new()
		{
			Id = 1,
			Title = new() { Romaji = Monster },
			Url = "https://anilist.co/anime/1",
			Format = format,
			SeasonYear = seasonYear,
			AverageScore = averageScore,
			Popularity = popularity,
			IsAdult = isAdult,
		};

	private static SearchMedia TitledMedia(
		string? romaji = null,
		string? english = null,
		string? native = null,
		IReadOnlyList<string>? synonyms = null) => new()
		{
			Id = 1,
			Title = new()
			{
				Romaji = romaji,
				English = english,
				Native = native,
			},
			Url = "https://anilist.co/anime/1",
			Synonyms = synonyms ?? [],
			Popularity = 0U,
		};
}
