// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models
{
    public sealed class ListActivity
    {
        [JsonPropertyName("status")]
        public string Status { get; init; } = null!;

        [JsonPropertyName("progress")]
        public string? Progress { get; init; } = null!;

        [JsonPropertyName("createdAt")]
        public long CreatedAtTimestamp { get; init; }

        [JsonPropertyName("media")]
        public Media Media { get; init; } = null!;
    }
}