// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using DSharpPlus.Entities;

namespace PaperMalKing.Shikimori.UpdateProvider;

public sealed record ShikiAchievement(string Id, byte Level, Uri Image, DiscordColor BorderColor, string TitleRussian, string? TextRussian, string TitleEnglish,
									  string? TextEnglish, string? HumanName);

public sealed class NekoFileJson
{
	public required Dictionary<string, string> HumanNames { get; init; }
	public required IReadOnlyList<ShikiAchievementJsonItem> Achievements { get; init; }
}

public sealed class ShikiAchievementJsonItem
{
	[JsonPropertyName("neko_id")]
	public required string Id { get; init; }

	[JsonPropertyName("level")]
	public required byte Level { get; init; }

	[JsonPropertyName("image")]
	public required string Image { get; init; }

	[JsonPropertyName("border_color")]
	public string? BorderColor { get; init; }

	[JsonPropertyName("title_en")]
	public required string TitleEnglish { get; init; }

	[JsonPropertyName("title_ru")]
	public required string TitleRussian { get; init; }

	[JsonPropertyName("text_en")]
	public string? TextEnglish { get; init; }

	[JsonPropertyName("text_ru")]
	public string? TextRussian { get; init; }
}