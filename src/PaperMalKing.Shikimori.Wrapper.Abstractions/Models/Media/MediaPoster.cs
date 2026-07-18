// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

public sealed class MediaPoster
{
	[JsonPropertyName("main2xUrl")]
	public string? Main2xUrl { get; init; }

	[JsonPropertyName("mainAlt2xUrl")]
	public string? MainAlt2xUrl { get; init; }

	[JsonPropertyName("mainAltUrl")]
	public string? MainAltUrl { get; init; }

	[JsonPropertyName("mainUrl")]
	public string? MainUrl { get; init; }

	[JsonPropertyName("mini2xUrl")]
	public string? Mini2xUrl { get; init; }

	[JsonPropertyName("miniAlt2xUrl")]
	public string? MiniAlt2xUrl { get; init; }

	[JsonPropertyName("miniAltUrl")]
	public string? MiniAltUrl { get; init; }

	[JsonPropertyName("miniUrl")]
	public string? MiniUrl { get; init; }

	[JsonPropertyName("originalUrl")]
	public string? OriginalUrl { get; init; }

	[JsonPropertyName("preview2xUrl")]
	public string? Preview2xUrl { get; init; }

	[JsonPropertyName("previewAlt2xUrl")]
	public string? PreviewAlt2xUrl { get; init; }

	[JsonPropertyName("previewAltUrl")]
	public string? PreviewAltUrl { get; init; }

	[JsonPropertyName("previewUrl")]
	public string? PreviewUrl { get; init; }

	public string? BestImageUrl
	{
		get
		{
			ReadOnlySpan<string?> imageUrls =
			[
				this.OriginalUrl, this.Main2xUrl, this.MainAlt2xUrl, this.Preview2xUrl, this.PreviewAlt2xUrl, this.Mini2xUrl, this.MiniAlt2xUrl,
				this.MainUrl, this.MainAltUrl, this.PreviewUrl, this.PreviewAltUrl, this.MiniUrl, this.MiniAltUrl,
			];

			for (var i = 0; i < imageUrls.Length; i++)
			{
				var url = imageUrls[i];

				if (!string.IsNullOrWhiteSpace(url) && !url.Contains(Constants.MissingImagePattern, StringComparison.Ordinal))
				{
					return url;
				}
			}

			return null;
		}
	}
}