// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.Collections.Generic;
using System.Text.Json.Serialization;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.Base;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;

public sealed class MangaListEntryNode : BaseListEntryNode<MangaMediaType, MangaPublishingStatus>
{
	private string? _url;
	public override uint TotalSubEntries => this.TotalChapters;

	[JsonPropertyName("num_volumes")]
	public required uint TotalVolumes { get; init; }

	[JsonPropertyName("num_chapters")]
	public required uint TotalChapters { get; init; }

	[JsonPropertyName("authors")]
	public IReadOnlyList<Author>? Authors { get; init; }

	public override string Url => this._url ??= $"{Constants.BASE_URL}/manga/{this.Id}";
}