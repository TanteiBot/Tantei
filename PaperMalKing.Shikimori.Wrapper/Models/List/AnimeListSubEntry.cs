// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models.List;

internal sealed class AnimeListSubEntry : BaseListSubEntry
{
	protected override string Type => "animes";

	public override string TotalAmount => $"{this.Episodes} ep.";

	[JsonPropertyName("episodes")]
	public int Episodes { get; init; }
}