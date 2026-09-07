// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models.Responses;

public sealed class MediaByIdResponse
{
	[JsonPropertyName("Media")]
	public SearchMedia? Media { get; init; }

	public static readonly MediaByIdResponse Empty = new()
	{
		Media = null,
	};
}
