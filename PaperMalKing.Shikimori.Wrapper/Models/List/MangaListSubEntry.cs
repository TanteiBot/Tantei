// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models.List;

internal sealed class MangaListSubEntry : BaseListSubEntry
{
	protected override string Type => "mangas";

	/// <inheritdoc />
	public override string TotalAmount => $"{this.Chapters} ep., {this.Volumes} v.";

	[JsonPropertyName("volumes")]
	public int Volumes { get; init; }

	[JsonPropertyName("chapters")]
	public int Chapters { get; init; }
}