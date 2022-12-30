// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models;
#pragma warning disable CA1711
public sealed class MediaListCollection
#pragma warning restore CA1711
{
	[JsonPropertyName("lists")]
	public IReadOnlyList<MediaListGroup> Lists { get; init; } = Array.Empty<MediaListGroup>();

	public static readonly MediaListCollection Empty = new();
}