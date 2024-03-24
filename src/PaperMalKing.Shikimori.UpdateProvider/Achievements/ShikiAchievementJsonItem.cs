// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.Common.Json;

namespace PaperMalKing.Shikimori.UpdateProvider.Achievements;

public sealed class ShikiAchievementJsonItem
{
	[JsonPropertyName("neko_id")]
	[JsonConverter(typeof(ClearableStringPoolingJsonConverter))]
	public required string Id { get; init; }

	[JsonPropertyName("level")]
	public required byte Level { get; init; }

	[JsonPropertyName("image")]
	[JsonConverter(typeof(ClearableStringPoolingJsonConverter))]
	public required string Image { get; init; }

	[JsonPropertyName("border_color")]
	[JsonConverter(typeof(HexNumberJsonConverter))]
	public int? BorderColor { get; init; }

	[JsonPropertyName("title_en")]
	[JsonConverter(typeof(ClearableStringPoolingJsonConverter))]
	public required string TitleEnglish { get; init; }

	[JsonPropertyName("title_ru")]
	[JsonConverter(typeof(ClearableStringPoolingJsonConverter))]
	public required string TitleRussian { get; init; }

	[JsonPropertyName("text_en")]
	public string? TextEnglish { get; init; }

	[JsonPropertyName("text_ru")]
	public string? TextRussian { get; init; }
}