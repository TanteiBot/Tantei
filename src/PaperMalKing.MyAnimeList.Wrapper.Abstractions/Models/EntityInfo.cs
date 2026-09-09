// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;

public sealed class EntityInfo
{
	public static EntityInfo Empty { get; } = new();

	public string? Description { get; init; }

	public BestKnownWork? BestKnownWork { get; init; }
}
