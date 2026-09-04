// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.Common.Exceptions;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Enums;

public static class ShikiKindExtensions
{
	public static string ToGraphQlKind(this AnimeKind kind) => kind switch
	{
		AnimeKind.Tv => "tv",
		AnimeKind.Movie => "movie",
		AnimeKind.Ova => "ova",
		AnimeKind.Ona => "ona",
		AnimeKind.Special => "special",
		AnimeKind.TvSpecial => "tv_special",
		AnimeKind.Music => "music",
		AnimeKind.Pv => "pv",
		AnimeKind.Cm => "cm",
		_ => ArgumentOutOfRangeException.Throw<string>(nameof(kind), kind, message: null),
	};

	public static string ToGraphQlKind(this MangaKind kind) => kind switch
	{
		MangaKind.Manga => "manga",
		MangaKind.Manhwa => "manhwa",
		MangaKind.Manhua => "manhua",
		MangaKind.LightNovel => "light_novel",
		MangaKind.Novel => "novel",
		MangaKind.OneShot => "one_shot",
		MangaKind.Doujin => "doujin",
		_ => ArgumentOutOfRangeException.Throw<string>(nameof(kind), kind, message: null),
	};
}
