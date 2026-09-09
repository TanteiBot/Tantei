// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models;

public sealed record FavouriteIds
{
	public IReadOnlyList<uint> AnimeIds { get; init; } = [];

	public IReadOnlyList<uint> MangaIds { get; init; } = [];

	public IReadOnlyList<uint> CharacterIds { get; init; } = [];

	public IReadOnlyList<uint> PersonIds { get; init; } = [];

	public bool IsEmpty => this.AnimeIds.Count == 0 && this.MangaIds.Count == 0 && this.CharacterIds.Count == 0 && this.PersonIds.Count == 0;
}
