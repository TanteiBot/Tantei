#region LICENSE
// PaperMalKing.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

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
			var errorMessage = message ?? $"{ex?.Message}\nin\n{Formatter.InlineCode(ex?.Source)}";
			return ErrorEmbed(user, errorMessage, $"Error occured in {command.Name}");
		}

		public static DiscordEmbedBuilder UnknownErrorEmbed(CommandContext context)
		{
			return ErrorEmbed(context, $"Unknown error occured, try again later or contact the owner in case of sequential fails", "Unknown Error");
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