// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models;

public sealed class FavouritesInfo
{
	public static readonly FavouritesInfo Empty = new();

	[JsonPropertyName("animes")]
	public IReadOnlyList<AnimeMedia> Animes { get; init; } = [];

	[JsonPropertyName("mangas")]
	public IReadOnlyList<MangaMedia> Mangas { get; init; } = [];

	[JsonPropertyName("characters")]
	public IReadOnlyList<FavouriteCharacter> Characters { get; init; } = [];

	[JsonPropertyName("people")]
	public IReadOnlyList<FavouritePerson> People { get; init; } = [];
}
