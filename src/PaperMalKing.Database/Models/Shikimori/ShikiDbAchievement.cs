// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

namespace PaperMalKing.Database.Models.Shikimori;

public sealed class ShikiDbAchievement
{
	public required string NekoId { get; init; }
	public required byte Level { get; set; }
}