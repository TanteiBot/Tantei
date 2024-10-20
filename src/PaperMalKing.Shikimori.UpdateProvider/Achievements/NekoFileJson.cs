﻿// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Collections.Generic;

namespace PaperMalKing.Shikimori.UpdateProvider.Achievements;

public sealed class NekoFileJson
{
	public IReadOnlyDictionary<string, string>? HumanNames { get; init; }

	public IReadOnlyList<ShikiAchievementJsonItem>? Achievements { get; init; }
}