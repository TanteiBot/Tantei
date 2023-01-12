// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace PaperMalKing.Common;

public abstract class BotCommandsModule : ApplicationCommandModule
{
	/// <inheritdoc />
	public override async Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
	{
		await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);
		return true;
	}
}