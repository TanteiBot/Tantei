// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Interfaces;

namespace PaperMalKing.AniList.Wrapper.Models
{
    public sealed class Studio : ISiteUrlable, IIdentifiable
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        [JsonPropertyName("siteUrl")]
        public string Url { get; init; } = null!;

        [JsonPropertyName("media")]
        public Connection<Media> Media { get; init; } = Connection<Media>.Empty;

        [JsonPropertyName("isAnimationStudio")]
        public bool IsAnimationStudio { get; init; }

        [JsonPropertyName("id")]
        public ulong Id { get; init; }
    }
}