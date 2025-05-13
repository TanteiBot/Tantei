// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

public sealed class HistoryTargetImage
{
	[JsonPropertyName("original")]
	public string? Original { get; init; }

	[JsonPropertyName("preview")]
	public string? Preview { get; init; }

	[JsonPropertyName("x96")]
	public string? X96 { get; init; }

	[JsonPropertyName("x48")]
	public string? X48 { get; init; }

	[JsonIgnore]
	public string? ImageUrl
	{
		get
		{
			var url = this.Original ?? this.Preview ?? this.X96 ?? this.X48;

			if (!string.IsNullOrWhiteSpace(url))
			{
				return Constants.BaseUrl + url;
			}

			return null;
		}
	}
}