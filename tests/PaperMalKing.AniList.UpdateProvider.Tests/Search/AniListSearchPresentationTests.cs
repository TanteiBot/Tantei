// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.AniList.UpdateProvider.Search;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

namespace PaperMalKing.AniList.UpdateProvider.Tests.Search;

public sealed class AniListSearchPresentationTests
{
	private const ushort SeasonYear = 2004;
	private const ushort AverageScore = 85;
	private const uint Popularity = 1_400_000;
	private const uint SmallPopularity = 999;
	private const uint ThousandsPopularity = 1_200;

	[Test]
	public async Task ComposesFormatYearScoreAndPopularity()
	{
		var media = Media(format: MediaFormat.Movie, seasonYear: SeasonYear, averageScore: AverageScore, popularity: Popularity);

		await Assert.That(AniListSearchPresentation.BuildOptionDescription(media, ScoreFormat.POINT_100)).IsEqualTo("Movie · 2004 · 85/100 · 1.4M");
	}

	[Test]
	public async Task RendersScoreInTheRequesterScoreFormat()
	{
		var media = Media(format: MediaFormat.Movie, seasonYear: SeasonYear, averageScore: AverageScore, popularity: Popularity);

		await Assert.That(AniListSearchPresentation.BuildOptionDescription(media, ScoreFormat.POINT_10_DECIMAL)).IsEqualTo("Movie · 2004 · 8.5/10 · 1.4M");
	}

	[Test]
	public async Task AdultResultsCarryTheBadge()
	{
		var media = Media(format: MediaFormat.Movie, seasonYear: SeasonYear, averageScore: AverageScore, popularity: Popularity, isAdult: true);

		await Assert.That(AniListSearchPresentation.BuildOptionDescription(media, ScoreFormat.POINT_100)).IsEqualTo("Movie · 2004 · 85/100 · 1.4M · 🔞");
	}

	[Test]
	public async Task NullScoreOmitsTheScoreToken()
	{
		var media = Media(format: MediaFormat.Movie, seasonYear: SeasonYear, averageScore: null, popularity: Popularity);

		await Assert.That(AniListSearchPresentation.BuildOptionDescription(media, ScoreFormat.POINT_100)).IsEqualTo("Movie · 2004 · 1.4M");
	}

	[Test]
	public async Task NullFormatAndYearAreOmitted()
	{
		var media = Media(format: null, seasonYear: null, averageScore: AverageScore, popularity: SmallPopularity);

		await Assert.That(AniListSearchPresentation.BuildOptionDescription(media, ScoreFormat.POINT_100)).IsEqualTo("85/100 · 999");
	}

	[Test]
	[Arguments(SmallPopularity, "999")]
	[Arguments(ThousandsPopularity, "1.2K")]
	[Arguments(Popularity, "1.4M")]
	public async Task PopularityIsHumanized(uint popularity, string expected)
	{
		await Assert.That(AniListSearchPresentation.FormatPopularity(popularity)).IsEqualTo(expected);
	}

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
