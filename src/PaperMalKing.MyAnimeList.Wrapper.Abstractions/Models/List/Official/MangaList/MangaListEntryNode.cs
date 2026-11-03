// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.Base;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;

public sealed class MangaListEntryNode : BaseListEntryNode<MangaMediaType, MangaPublishingStatus>
{
	public override uint TotalSubEntries => this.TotalChapters;

	[JsonPropertyName("num_volumes")]
	public required uint TotalVolumes { get; init; }

	[JsonPropertyName("num_chapters")]
	public required uint TotalChapters { get; init; }

	[JsonPropertyName("authors")]
	public IReadOnlyList<Author>? Authors { get; init; }

	[field: MaybeNull]
	[field: AllowNull]
	public override string Url => field ??= $"{Constants.BaseUrl}/manga/{this.Id}";
}