// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models.Responses;

public sealed class MediaSearchResponse
{
	[JsonPropertyName("Page")]
	public required Page<SearchMedia> Page { get; init; }

	[JsonPropertyName("User")]
	public User? User { get; init; }

	public static readonly MediaSearchResponse Empty = new()
	{
		Page = Page<SearchMedia>.Empty,
		User = null,
	};
}
