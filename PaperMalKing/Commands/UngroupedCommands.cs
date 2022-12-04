#region LICENSE

// PaperMalKing.
// Copyright (C) 2021-2022 N0D4N
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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Hosting;
using PaperMalKing.Common;
using PaperMalKing.Common.Attributes;
using PaperMalKing.Services;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.Commands
{
	[ModuleLifespan(ModuleLifespan.Singleton)]
	[SuppressMessage("Performance", "CA1822")]
	[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods")]
	public sealed class UngroupedCommands : BaseCommandModule
	{
		private readonly UpdateProvidersConfigurationService _providersConfigurationService;
		private readonly IHostApplicationLifetime _lifetime;

		private static DiscordEmbed? AboutEmbed;

		public UngroupedCommands(UpdateProvidersConfigurationService providersConfigurationService, IHostApplicationLifetime lifetime)
		{
			this._providersConfigurationService = providersConfigurationService;
			this._lifetime = lifetime;
		}

		[Command("say")]
		[Description("Sends embed in selected channel with selected text")]
		[OwnerOrPermissions(Permissions.ManageGuild)]
		public async Task SayCommand(CommandContext context, [Description("Channel where the embed will be send")] DiscordChannel channelToSayIn,
									 [RemainingText, Description("Text to send")] string messageContent)
		{
			if (string.IsNullOrWhiteSpace(messageContent))
				throw new ArgumentException("Message's content shouldn't be empty", nameof(messageContent));
			var embed = new DiscordEmbedBuilder
			{
				Description = messageContent.Replace("@everyone", "@\u200beveryone", StringComparison.Ordinal)
											.Replace("@here", "@\u200bhere", StringComparison.Ordinal),
				Timestamp = DateTime.Now,
				Color = DiscordColor.Blue
			}.WithAuthor($"{context.Member!.Username}#{context.Member.Discriminator}", iconUrl: context.Member!.AvatarUrl);
			try
			{
				await channelToSayIn.SendMessageAsync(embed: embed).ConfigureAwait(false);
			}
			#pragma warning disable CA1031
			catch
				#pragma warning restore CA1031
			{
				await context.RespondAsync("Couldn't send message. Check permissions for bot and try again.").ConfigureAwait(false);
			}
		}

		[Command("About")]
		[Description("Displays info about bot")]
		[Aliases("Info")]
		public Task AboutCommand(CommandContext context)
		{
			if (AboutEmbed == null)
			{
				var botVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "";
				var dotnetVersion = Environment.Version.ToString(3);

				var commitId = ThisAssembly.GitCommitId[..10];
				var commitDate = ThisAssembly.GitCommitDate;
				const string desc = """
									Tantei is bot designed to automatically track and send to Discord its users updates from MyAnimeList, AniList, Shikimori.
									Developed by N0D4N#2281 (<@356518417987141633>).
									""";

				const string sourceCodeLink = "https://github.com/TanteiBot/Tantei";

				var versions = $"""
								Bot version - {botVersion}
								Commit - {Formatter.MaskedUrl(commitId, new Uri($"{sourceCodeLink}/commit/{commitId.AsSpan(0, 10)}"))}
								Commit date - {commitDate:s}
								DSharpPlus version - {context.Client.VersionString}
								.NET version - {dotnetVersion}
								""";

				var links = Formatter.MaskedUrl("Source code", new Uri(sourceCodeLink, UriKind.Absolute));

				var embedBuilder = new DiscordEmbedBuilder
				{
					Title = "Info",
					Url = sourceCodeLink,
					Description = desc,
					Color = DiscordColor.DarkBlue,
				}.WithThumbnail(context.Client.CurrentUser.AvatarUrl);
				embedBuilder.AddField("Links", links, true);
				embedBuilder.AddField("Versions", versions, true);
				Interlocked.Exchange(ref AboutEmbed, embedBuilder.Build());
			}

			return context.RespondAsync(embed: AboutEmbed);
		}

		[Command("DeleteMessages")]
		[Aliases("dmsg", "rm", "rmm")]
		[Description("Delete messages by their id")]
		[RequireOwner]
		public async Task DeleteMessagesCommand(CommandContext context, [RemainingText, Description("Messages Id's")] params ulong[] messages)
		{
			var msgsToDelete =
				(await context.Channel.GetMessagesBeforeAsync(context.Message.Id).ConfigureAwait(false)).Where(x =>
					x.Author.IsCurrent && messages.Contains(x.Id));
			foreach (var msg in msgsToDelete)
				await context.Channel.DeleteMessageAsync(msg).ConfigureAwait(false);
		}

		[Command("Forcecheck")]
		[Aliases("fc")]
		[RequireOwner]
		public async Task ForceCheckCommand(CommandContext context, [RemainingText, Description("Update provider name")] string name)
		{
			name = name.Trim();
			BaseUpdateProvider? baseUpdateProvider;
			if (this._providersConfigurationService.Providers.TryGetValue(name, out var provider) && provider is BaseUpdateProvider bup)
			{
				baseUpdateProvider = bup;
			}
			else
			{
				var upc = this._providersConfigurationService.Providers.Values.FirstOrDefault(p => p.Name.Where(char.IsUpper).ToString() == name);
				baseUpdateProvider = upc as BaseUpdateProvider;
			}

			if (baseUpdateProvider != null)
			{
				baseUpdateProvider.RestartTimer(TimeSpan.Zero);
				await context.RespondAsync(embed: EmbedTemplate.SuccessEmbed(context, "Success")).ConfigureAwait(false);
			}
			else
			{
				await context.RespondAsync(embed: EmbedTemplate.ErrorEmbed(context, "Haven't found such update provider")).ConfigureAwait(false);
			}
		}

		[Command("stop")]
		[Aliases("restart")]
		[RequireOwner]
		public Task StopBotCommand(CommandContext context)
		{
			this._lifetime.StopApplication();
			return Task.CompletedTask;
		}
	}
}