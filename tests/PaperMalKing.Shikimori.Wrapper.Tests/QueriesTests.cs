// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Enums;

namespace PaperMalKing.Shikimori.Wrapper.Tests;

public sealed class QueriesTests
{
	[Test]
	public async Task AnimeSearchWithoutKindOmitsTheKindArgument()
	{
		var query = Queries.GetAnimeSearchQuery(kind: null, includeNsfw: false);

		await Assert.That(query).Contains("media: animes (search: $search, limit: 50, censored: true)");
		await Assert.That(query).DoesNotContain("kind:");
	}

	[Test]
	public async Task AnimeSearchWithKindEmitsTheGraphQlKindToken()
	{
		var query = Queries.GetAnimeSearchQuery(AnimeKind.TvSpecial, includeNsfw: false);

		await Assert.That(query).Contains("kind: \"tv_special\"");
	}

	[Test]
	public async Task AnimeSearchInNsfwChannelDisablesCensorship()
	{
		var query = Queries.GetAnimeSearchQuery(kind: null, includeNsfw: true);

		await Assert.That(query).Contains("censored: false");
		await Assert.That(query).DoesNotContain("censored: true");
	}

	[Test]
	public async Task AnimeSearchSelectsIdentityAndEnrichmentFields()
	{
		var query = Queries.GetAnimeSearchQuery(kind: null, includeNsfw: false);

		foreach (var field in new[] { "id", "name", "russian", "english", "japanese", "synonyms", "kind", "score", "status", "airedOn { year }", "statusesStats { count }", "url", "rating", "studios" })
		{
			await Assert.That(query).Contains(field);
		}
	}

	[Test]
	public async Task MangaSearchWithKindEmitsTheGraphQlKindTokenAndSelectsPublishers()
	{
		var query = Queries.GetMangaSearchQuery(MangaKind.LightNovel, includeNsfw: false);

		await Assert.That(query).Contains("media: mangas (search: $search, limit: 50, censored: true, kind: \"light_novel\")");
		await Assert.That(query).Contains("publishers");
	}

	[Test]
	public async Task MangaSearchInNsfwChannelDisablesCensorship()
	{
		var query = Queries.GetMangaSearchQuery(kind: null, includeNsfw: true);

		await Assert.That(query).Contains("censored: false");
	}
}
