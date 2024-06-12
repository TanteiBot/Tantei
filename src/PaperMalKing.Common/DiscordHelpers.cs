// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace PaperMalKing.Common;

public static class DiscordHelpers
{
	public static string ToDiscordMention(ulong id) => $"<@!{id}>";

	public static Task<DiscordMessage> EditResponseAsync(this BaseContext context, DiscordEmbed embed)
	{
		return context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
	}
}