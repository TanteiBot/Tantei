// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.MyAnimeList.UpdateProvider.Search;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;
using TUnit.Assertions.Enums;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;

public sealed class SearchResultRankerTests
{
	private const string Monster = "Monster";

	[Test]
	public async Task CandidateKeysDiscardEmptyValuesAndDeduplicateByBestTitleSource()
	{
		var result = Anime(
			1U,
			"K-On!",
			synonyms: ["K On", string.Empty, null, "!!!"],
			japanese: "Ｋ－ＯＮ！",
			english: "K-On");

		var candidates = SearchResultRanker.CreateCandidateKeys(result);

		await Assert.That(candidates).HasSingleItem();
		await Assert.That(candidates[0].Key.Value).IsEqualTo("KON");
		await Assert.That(candidates[0].ExactRank).IsEqualTo(MatchRank.Primary);
	}

	[Test]
	public async Task BestRankWinsAcrossEveryTitleSource()
	{
		var query = MatchKey.Create(Monster);
		var results = new[]
		{
			Anime(1U, Monster, synonyms: ["Monster Story"]),
			Anime(2U, "Pocket Monster", synonyms: [Monster]),
			Anime(3U, "Monster Story", synonyms: ["Pocket Monster"], japanese: Monster),
			Anime(4U, "Monster Story", synonyms: ["Pocket Monster"], japanese: "Kaibutsu", english: Monster),
			Anime(5U, "Pocket Monsters"),
			Anime(6U, "Kaibutsu"),
		};

		var ranks = results.Select(result => SearchResultRanker.GetMatchRank(query, result));

		await Assert.That(ranks).IsEquivalentTo(
			[MatchRank.Primary, MatchRank.Synonym, MatchRank.Japanese, MatchRank.English, MatchRank.Contains, MatchRank.None,],
			CollectionOrdering.Matching);
	}

	[Test]
	public async Task ContainmentUsesTheNormalizedOrdinalMatchKey()
	{
		var kon = SearchResultRanker.GetMatchRank(MatchKey.Create("kon"), Anime(1U, "K-On!"));
		var toomMiss = SearchResultRanker.GetMatchRank(MatchKey.Create("Toom"), Anime(2U, "Btooom!"));
		var toomMatch = SearchResultRanker.GetMatchRank(MatchKey.Create("Toom"), Anime(3U, "Boomba & Toomba"));
		var deletedBoundaryMatch = SearchResultRanker.GetMatchRank(MatchKey.Create("OHARU"), Anime(4U, "Ao Haru Ride"));

		await Assert.That(kon).IsEqualTo(MatchRank.Primary);
		await Assert.That(toomMiss).IsEqualTo(MatchRank.None);
		await Assert.That(toomMatch).IsEqualTo(MatchRank.Contains);
		await Assert.That(deletedBoundaryMatch).IsEqualTo(MatchRank.Contains);
	}

	internal static AnimeSearchResult Anime(
		uint id,
		string title,
		uint listUserCount = 0U,
		AnimeMediaType mediaType = AnimeMediaType.TV,
		IReadOnlyList<string?>? synonyms = null,
		string? japanese = null,
		string? english = null) => new()
		{
			Id = id,
			PrimaryTitle = title,
			MediaType = mediaType,
			Status = AnimeAiringStatus.Unknown,
			Episodes = 0U,
			ListUserCount = listUserCount,
			Genres = [],
			AlternativeTitles = synonyms is null && japanese is null && english is null
				? null
				: new()
				{
					Synonyms = synonyms,
					Japanese = japanese,
					English = english,
				},
		};
}
