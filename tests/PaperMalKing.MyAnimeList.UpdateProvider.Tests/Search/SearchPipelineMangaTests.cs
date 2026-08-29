// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.MyAnimeList.UpdateProvider.Search;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;

public sealed class SearchPipelineMangaTests
{
	[Test]
	public async Task MangaMediaTypeFilterUsesTheSameRankingPipeline()
	{
		const uint expectedId = 2U;
		var response = new MangaSearchResponse
		{
			Results =
			[
				Envelope(1U, MangaMediaType.Manga),
				Envelope(expectedId, MangaMediaType.Novel),
			],
		};

		var outcome = SearchPipeline.Evaluate(MatchKey.Create("Monster"), response, MangaMediaType.Novel);

		await Assert.That(outcome.Kind).IsEqualTo(SearchOutcomeKind.AutoPost);
		await Assert.That(outcome.AutoPostResult).IsNotNull();
		await Assert.That(outcome.AutoPostResult!.Result.Id).IsEqualTo(expectedId);
	}

	private static SearchResultEnvelope<MangaSearchResult> Envelope(uint id, MangaMediaType mediaType) => new()
	{
		Result = new()
		{
			Id = id,
			PrimaryTitle = "Monster",
			MediaType = mediaType,
			Status = MangaPublishingStatus.Unknown,
			Chapters = 0U,
			Volumes = 0U,
			ListUserCount = 0U,
			Genres = [],
		},
	};
}
