// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace PaperMalKing.Common;

[SuppressMessage("Minor Code Smell", "S1694:An abstract class should have both abstract and concrete methods", Justification = "We only provide common functionality to inheritors")]
public abstract class BotCommandsModule : ApplicationCommandModule
{
	public override async Task<bool> BeforeSlashExecutionAsync(InteractionContext ctx)
	{
		await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
		return true;
	}
}