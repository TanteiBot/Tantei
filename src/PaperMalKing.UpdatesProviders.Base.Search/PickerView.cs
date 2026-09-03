// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;

namespace PaperMalKing.UpdatesProviders.Base.Search;

internal sealed record PickerView(string Content, IReadOnlyList<IReadOnlyList<DiscordComponent>> Rows)
{
	public static PickerView Terminal(string content) => new(content, []);
}
