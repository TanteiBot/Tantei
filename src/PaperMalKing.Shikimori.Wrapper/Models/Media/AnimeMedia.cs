// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models.Media;

internal class AnimeMedia : BaseMedia
{
	[JsonPropertyName("episodes")]
	public required uint Episodes { get; init; }

	[JsonPropertyName("studios")]
	public IReadOnlyList<Studio> Studios { get; init; } = Array.Empty<Studio>();

	protected override string Type => "animes";
}