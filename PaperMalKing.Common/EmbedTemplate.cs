// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace PaperMalKing.Common
{
	public static class EmbedTemplate
	{
		public static DiscordEmbedBuilder CommandErrorEmbed(Command command, DiscordUser user, Exception? ex = null,
			string? message = null)
		{
			var errorMessage = message ?? $"""
										   {ex?.Message}
										   in
										   {Formatter.InlineCode(ex?.Source)}
										   """;
			return ErrorEmbed(user, errorMessage, $"Error occured in {command.Name}");
		}

		public static DiscordEmbedBuilder UnknownErrorEmbed(CommandContext context)
		{
			return ErrorEmbed(context, "Unknown error occured, try again later or contact the owner in case of sequential fails", "Unknown Error");
		}

		public static DiscordEmbedBuilder ErrorEmbed(CommandContext context, string errorMessage, string? title = null) =>
			ErrorEmbed(context.User, errorMessage, title);

		public static DiscordEmbedBuilder ErrorEmbed(DiscordUser user, string errorMessage, string? title = null)
		{
			var embedBuilder = new DiscordEmbedBuilder
			{
				Author = new()
				{
					IconUrl = user.AvatarUrl,
					Name = user.Username
				},
				Title = title ?? "Error occured",
				Description = errorMessage,
				Timestamp = DateTimeOffset.Now,
				Color = DiscordColor.Red
			};
			return embedBuilder;
		}

		public static DiscordEmbedBuilder SuccessEmbed(CommandContext context, string message) => SuccessEmbed(context.User, message);
		public static DiscordEmbedBuilder SuccessEmbed(DiscordUser user, string message)
		{
			var embedBuilder = new DiscordEmbedBuilder
			{
				Author = new()
				{
					IconUrl = user.AvatarUrl,
					Name = user.Username
				},
				Timestamp = DateTimeOffset.Now,
				Color = new DiscordColor("#10c710")
			};
			return message.Length > 256 ? embedBuilder.WithDescription(message) : embedBuilder.WithTitle(message);
		}
	}
}