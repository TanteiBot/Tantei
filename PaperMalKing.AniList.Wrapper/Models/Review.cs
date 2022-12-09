// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Enums;
using PaperMalKing.AniList.Wrapper.Models.Interfaces;

namespace PaperMalKing.AniList.Wrapper.Models
{
	public sealed class Review : ISiteUrlable
	{
		[JsonPropertyName("createdAt")]
		public long CreatedAtTimeStamp { get; init; }

		[JsonPropertyName("siteUrl")]
		public string Url { get; init; } = null!;

		[JsonPropertyName("summary")]
		public string? Summary { get; init; }

		[JsonPropertyName("media")]
		public Media Media { get; init; } = null!;

		[JsonPropertyName("format")]
		public MediaFormat Format { get; init; }
	}
}