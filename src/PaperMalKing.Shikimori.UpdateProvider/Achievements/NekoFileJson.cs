// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Collections.ObjectModel;

namespace PaperMalKing.Shikimori.UpdateProvider.Achievements;

public sealed class NekoFileJson
{
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
	public IReadOnlyDictionary<string, string> HumanNames { get; init; } = ReadOnlyDictionary<string, string>.Empty;
#pragma warning restore CA1859

	public IReadOnlyList<ShikiAchievementJsonItem> Achievements { get; init; } = [];
}