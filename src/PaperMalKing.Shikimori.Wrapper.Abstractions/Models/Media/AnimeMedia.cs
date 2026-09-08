// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

public class AnimeMedia : BaseMedia
{
	[JsonPropertyName("studios")]
	public IReadOnlyList<Studio> Studios { get; init; } = [];

	[JsonPropertyName("episodes")]
	public uint? Episodes { get; init; }

	[JsonPropertyName("episodesAired")]
	public uint? EpisodesAired { get; init; }

	protected override string Type => "animes";
}
