// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Enums;

namespace PaperMalKing.Shikimori.Wrapper.Tests;

public sealed class ShikiKindExtensionsTests
{
	[Test]
	[Arguments(AnimeKind.Tv, "tv")]
	[Arguments(AnimeKind.Movie, "movie")]
	[Arguments(AnimeKind.Ova, "ova")]
	[Arguments(AnimeKind.Ona, "ona")]
	[Arguments(AnimeKind.Special, "special")]
	[Arguments(AnimeKind.TvSpecial, "tv_special")]
	[Arguments(AnimeKind.Music, "music")]
	[Arguments(AnimeKind.Pv, "pv")]
	[Arguments(AnimeKind.Cm, "cm")]
	public async Task AnimeKindMapsToItsGraphQlToken(AnimeKind kind, string expected)
	{
		await Assert.That(kind.ToGraphQlKind()).IsEqualTo(expected);
	}

	[Test]
	[Arguments(MangaKind.Manga, "manga")]
	[Arguments(MangaKind.Manhwa, "manhwa")]
	[Arguments(MangaKind.Manhua, "manhua")]
	[Arguments(MangaKind.LightNovel, "light_novel")]
	[Arguments(MangaKind.Novel, "novel")]
	[Arguments(MangaKind.OneShot, "one_shot")]
	[Arguments(MangaKind.Doujin, "doujin")]
	public async Task MangaKindMapsToItsGraphQlToken(MangaKind kind, string expected)
	{
		await Assert.That(kind.ToGraphQlKind()).IsEqualTo(expected);
	}

	[Test]
	public async Task UnknownKindThrows()
	{
		await Assert.That(static () => AnimeKind.Unknown.ToGraphQlKind()).Throws<ArgumentOutOfRangeException>();
		await Assert.That(static () => MangaKind.Unknown.ToGraphQlKind()).Throws<ArgumentOutOfRangeException>();
	}
}
