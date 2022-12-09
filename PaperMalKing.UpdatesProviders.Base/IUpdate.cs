// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.Collections.Generic;
using DSharpPlus.Entities;

namespace PaperMalKing.UpdatesProviders.Base;

public interface IUpdate
{
	IReadOnlyList<DiscordEmbedBuilder> UpdateEmbeds { get; }
}