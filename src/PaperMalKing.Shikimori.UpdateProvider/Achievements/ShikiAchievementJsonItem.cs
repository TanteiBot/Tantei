// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Diagnostics.CodeAnalysis;

namespace PaperMalKing.Shikimori.UpdateProvider.Achievements;

[SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "This is used for configuration which does not respect JsonPropertyName")]
public sealed class ShikiAchievementJsonItem
{
	public required string neko_id { get; init; }

	public required byte level { get; init; }

	public required string image { get; init; }

	public int? border_color { get; init; }

	public required string title_en { get; init; }

	public required string title_ru { get; init; }

	public string? text_en { get; init; }

	public string? text_ru { get; init; }
}