// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models.Responses;

public sealed class MediaSearchResponse
{
	[JsonPropertyName("Media")]
	public required Media? Media { get; init; }

	[JsonPropertyName("User")]
	public User? User { get; init; }

	public static readonly MediaSearchResponse Empty = new()
	{
		Media = null,
		User = null,
	};
}