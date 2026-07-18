// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

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
			ReadOnlySpan<string?> imageUrls = [this.Original, this.Preview, this.X96, this.X48];

			for (var i = 0; i < imageUrls.Length; i++)
			{
				var url = imageUrls[i];

				if (!string.IsNullOrWhiteSpace(url) && !url.Contains(Constants.MissingImagePattern, StringComparison.Ordinal))
				{
					return Constants.BaseUrl + url;
				}
			}

			return null;
		}
	}
}