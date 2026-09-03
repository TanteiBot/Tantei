// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;
using PaperMalKing.AniList.UpdateProvider.Search;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

namespace PaperMalKing.AniList.UpdateProvider.Tests.Search;

public sealed class AniListMediaCandidateTests
{
	private const ushort SeasonYear = 2004;
	private const ushort AverageScore = 85;
	private const uint Popularity = 1_400_000;
	private const uint SmallPopularity = 999;

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
			Title = new() { Romaji = "Monster" },
			Url = "https://anilist.co/anime/1",
			Format = format,
			SeasonYear = seasonYear,
			AverageScore = averageScore,
			Popularity = popularity,
			IsAdult = isAdult,
		};
}
