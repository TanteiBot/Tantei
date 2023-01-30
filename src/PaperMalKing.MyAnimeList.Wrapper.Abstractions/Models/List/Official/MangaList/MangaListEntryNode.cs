// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.Collections.Generic;
using System.Text.Json.Serialization;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.Base;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;

public sealed class MangaListEntryNode : BaseListEntryNode<MangaMediaType, MangaPublishingStatus>
{
	private string? _url = null;
	public override uint TotalSubEntries => this.TotalChapters;

	[JsonPropertyName("num_volumes")]
	[JsonRequired]
	public uint TotalVolumes { get; internal set; }

	[JsonPropertyName("num_chapters")]
	[JsonRequired]
	public uint TotalChapters { get; internal set; }

	[JsonPropertyName("authors")]
	public IReadOnlyList<Author>? Authors { get; internal set; }

	public override string Url => this._url ??= $"{Constants.BASE_URL}/manga/{this.Id}";
}