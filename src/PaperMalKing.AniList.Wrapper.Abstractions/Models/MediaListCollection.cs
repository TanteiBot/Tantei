// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models;

public sealed class MediaListCollection : IReadOnlyList<MediaListGroup>
{
	[JsonPropertyName("lists")]
	public IReadOnlyList<MediaListGroup> Lists { get; init; } = [];

	public static readonly MediaListCollection Empty = new();

	public IEnumerator<MediaListGroup> GetEnumerator() => this.Lists.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.Lists).GetEnumerator();

	public int Count => this.Lists.Count;

	public MediaListGroup this[int index] => this.Lists[index];
}