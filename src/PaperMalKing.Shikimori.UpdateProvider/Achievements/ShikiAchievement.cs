// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;

namespace PaperMalKing.Shikimori.UpdateProvider.Achievements;

public sealed record ShikiAchievement(string Id, byte Level, Uri Image, DiscordColor BorderColor, string TitleRussian, string? TextRussian, string TitleEnglish,
									  string? TextEnglish, string? HumanName);