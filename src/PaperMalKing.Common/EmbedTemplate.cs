// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace PaperMalKing.Common;

public static class EmbedTemplate
{
	public static DiscordEmbed UnknownErrorEmbed { get; } =
		ErrorEmbed("Unknown error occured, try again later or contact the owner in case of sequential fails", "Unknown Error");

	public static DiscordEmbedBuilder ErrorEmbed(string errorMessage, string? title = null)
	{
		var embedBuilder = new DiscordEmbedBuilder
		{
			Title = title ?? "Error occured",
			Description = errorMessage,
			Color = DiscordColor.Red
		};
		return embedBuilder;
	}

	public static DiscordEmbedBuilder SuccessEmbed(string message)
	{
		var embedBuilder = new DiscordEmbedBuilder
		{
			Color = new DiscordColor("#10c710")
		};
		return message.Length > 256 ? embedBuilder.WithDescription(message) : embedBuilder.WithTitle(message);
	}
}