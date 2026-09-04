// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.Common.Enums;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.UpdateProvider.Search;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;
using PaperMalKing.UpdatesProviders.Base.Search;

namespace PaperMalKing.Shikimori.UpdateProvider.Tests.Search;

public sealed class ShikiMediaCandidateTests
{
	[Test]
	public async Task OptionDescriptionComposesKindYearScoreAndPopularity()
	{
		var media = Anime(kind: "tv_special", year: 2004, score: 8.5f, counts: [500, 499]);

		var candidate = ShikiMediaCandidate.Create(media, ListEntryType.Anime, ShikiUserFeatures.None, useRussian: false, requestedKindToken: null);

		await Assert.That(candidate.OptionDescription).IsEqualTo("Tv special · 2004 · ★ 8.5 · 999");
	}

	[Test]
	public async Task OptionDescriptionOmitsMissingScoreAndYear()
	{
		var media = Anime(kind: "tv", year: null, score: 0f, counts: [10]);

		var candidate = ShikiMediaCandidate.Create(media, ListEntryType.Anime, ShikiUserFeatures.None, useRussian: false, requestedKindToken: null);

		await Assert.That(candidate.OptionDescription).IsEqualTo("Tv · 10");
	}

	[Test]
	public async Task PopularityIsTheSumOfStatusCounts()
	{
		const long expectedPopularity = 6L;
		var media = Anime(kind: "tv", year: 2000, score: 7f, counts: [1, 2, 3]);

		var candidate = ShikiMediaCandidate.Create(media, ListEntryType.Anime, ShikiUserFeatures.None, useRussian: false, requestedKindToken: null);

		await Assert.That(candidate.Popularity).IsEqualTo(expectedPopularity);
	}

	[Test]
	public async Task MatchTitlesCarryTheExpectedRanks()
	{
		var media = Anime(kind: "tv", year: 2000, score: 7f, counts: [1], russian: "Монстр", english: "Monster", japanese: "モンスター", synonyms: ["Monsutaa"]);

		var candidate = ShikiMediaCandidate.Create(media, ListEntryType.Anime, ShikiUserFeatures.None, useRussian: false, requestedKindToken: null);

		await Assert.That(candidate.MatchTitles).Contains((media.Name, MatchRank.Primary));
		await Assert.That(candidate.MatchTitles).Contains(("Monsutaa", MatchRank.Synonym));
		await Assert.That(candidate.MatchTitles).Contains(("モンスター", MatchRank.Native));
		await Assert.That(candidate.MatchTitles).Contains(("Monster", MatchRank.English));
	}

	[Test]
	public async Task PrimaryTitleHonorsTheRussianPreference()
	{
		var media = Anime(kind: "tv", year: 2000, score: 7f, counts: [1], russian: "Монстр");

		var russian = ShikiMediaCandidate.Create(media, ListEntryType.Anime, ShikiUserFeatures.Russian, useRussian: true, requestedKindToken: null);
		var romaji = ShikiMediaCandidate.Create(media, ListEntryType.Anime, ShikiUserFeatures.None, useRussian: false, requestedKindToken: null);

		await Assert.That(russian.PrimaryTitle).IsEqualTo("Монстр");
		await Assert.That(romaji.PrimaryTitle).IsEqualTo(media.Name);
	}

	[Test]
	public async Task PassesTypeFilterComparesTheMediaKindToTheRequestedToken()
	{
		var media = Anime(kind: "tv", year: 2000, score: 7f, counts: [1]);

		var matching = ShikiMediaCandidate.Create(media, ListEntryType.Anime, ShikiUserFeatures.None, useRussian: false, requestedKindToken: "tv");
		var mismatched = ShikiMediaCandidate.Create(media, ListEntryType.Anime, ShikiUserFeatures.None, useRussian: false, requestedKindToken: "movie");
		var unfiltered = ShikiMediaCandidate.Create(media, ListEntryType.Anime, ShikiUserFeatures.None, useRussian: false, requestedKindToken: null);

		await Assert.That(matching.PassesTypeFilter).IsTrue();
		await Assert.That(mismatched.PassesTypeFilter).IsFalse();
		await Assert.That(unfiltered.PassesTypeFilter).IsTrue();
	}

	private static AnimeSearchMedia Anime(
		string kind,
		int? year,
		float score,
		long[] counts,
		string name = "Monster",
		string? russian = null,
		string? english = null,
		string? japanese = null,
		IReadOnlyList<string>? synonyms = null) => new()
		{
			Id = 1UL,
			Name = name,
			RussianName = russian,
			EnglishName = english,
			JapaneseName = japanese,
			Synonyms = synonyms ?? [],
			Kind = kind,
			Score = score,
			Status = "released",
			AiredOn = year is { } y ? new() { Year = y } : null,
			StatusesStats = [.. counts.Select(static count => new StatusStat { Count = count })],
			Url = "https://shikimori.io/animes/1",
		};
}
