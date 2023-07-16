// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Collections.Generic;
using DSharpPlus.Entities;

namespace PaperMalKing.UpdatesProviders.Base;

public class BaseUpdate : IUpdate
{
	public BaseUpdate(IReadOnlyList<DiscordEmbedBuilder> updateEmbeds)
	{
		this.UpdateEmbeds = updateEmbeds;
	}

	public IReadOnlyList<DiscordEmbedBuilder> UpdateEmbeds { get; }
}