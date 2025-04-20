// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

namespace PaperMalKing.Shikimori.Wrapper.Responses;

internal sealed class MediaResponse<T>
	where T : BaseMedia
{
	[JsonPropertyName("media")]
	public required IReadOnlyList<T> Media { get; init; }
}