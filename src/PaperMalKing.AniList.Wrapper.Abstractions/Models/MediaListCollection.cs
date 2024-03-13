// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models;

[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "We match AniList name here")]
public sealed class MediaListCollection
{
	[JsonPropertyName("lists")]
	public IReadOnlyList<MediaListGroup> Lists { get; init; } = [];

	public static readonly MediaListCollection Empty = new();
}