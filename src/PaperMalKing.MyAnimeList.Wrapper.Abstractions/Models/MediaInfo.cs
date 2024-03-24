// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Collections.Generic;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;

public sealed class MediaInfo
{
	public static MediaInfo Empty { get; } = new()
	{
		Demographic = [],
		Themes = [],
	};

	public required IReadOnlyList<string> Themes { get; init; }

	public required IReadOnlyList<string> Demographic { get; init; }
}