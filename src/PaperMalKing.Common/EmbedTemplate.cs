﻿// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using DSharpPlus.Entities;

namespace PaperMalKing.Common;

public static class EmbedTemplate
{
	private static readonly Optional<DiscordColor> RedColor = DiscordColor.Red;
	private static readonly Optional<DiscordColor> GreenishColor = new DiscordColor("#10c710");

	public static DiscordEmbed UnknownErrorEmbed { get; } =
		ErrorEmbed("Unknown error occured, try again later or contact the owner in case of sequential fails", "Unknown Error");

	public static DiscordEmbedBuilder ErrorEmbed(string errorMessage, string? title = null)
	{
		return new DiscordEmbedBuilder
		{
			Title = title ?? "Error occured",
			Description = errorMessage,
			Color = RedColor,
		};
	}

	public static DiscordEmbedBuilder SuccessEmbed(string message)
	{
		var embedBuilder = new DiscordEmbedBuilder
		{
			Color = GreenishColor,
		};
		return message.Length > 256 ? embedBuilder.WithDescription(message) : embedBuilder.WithTitle(message);
	}
}