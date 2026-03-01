// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;

namespace PaperMalKing.UpdatesProviders.Base;

public sealed class UpdateContents
{
	public required DiscordEmbedBuilder EmbedBuilder { get; init; }

	public UpdateFile[] Files { get; init; } = [];
}