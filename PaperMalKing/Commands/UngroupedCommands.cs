// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.Hosting;
using PaperMalKing.Common;
using PaperMalKing.Common.Attributes;
using PaperMalKing.Services;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.Commands;

[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods")]
public sealed class UngroupedCommands : ApplicationCommandModule
{
	private static DiscordEmbed? AboutEmbed;

	[SlashCommand("say", "Sends embed in selected channel with selected text", true)]
	[OwnerOrPermissions(Permissions.ManageGuild)]
	public async Task SayCommand(InteractionContext context, [Option("channel", "Channel where the embed will be send")] DiscordChannel channelToSayIn,
								 [Option("text", "Text to send")] string messageContent)
	{
		if (string.IsNullOrWhiteSpace(messageContent))
			throw new ArgumentException("Message's content shouldn't be empty", nameof(messageContent));
		var embed = new DiscordEmbedBuilder
		{
			Description = messageContent.Replace("@everyone", "@\u200beveryone", StringComparison.Ordinal)
										.Replace("@here", "@\u200bhere", StringComparison.Ordinal),
			Timestamp = DateTime.Now,
			Color = DiscordColor.Blue
		}.WithAuthor($"{context.User.Username}#{context.User.Discriminator}", iconUrl: context.User.AvatarUrl);
		try
		{
			await channelToSayIn.SendMessageAsync(embed: embed).ConfigureAwait(false);
		}
#pragma warning disable CA1031
		catch
#pragma warning restore CA1031
		{
			await context.CreateResponseAsync("Couldn't send message. Check permissions for bot and try again.").ConfigureAwait(false);
		}
	}

	[SlashCommand("About","Displays info about bot", true)]
	public Task AboutCommand(InteractionContext context)
	{
		if (AboutEmbed is null)
		{
			var owners = context.Client.CurrentApplication.Owners.Select(x => $"{x.Username}#{x.Discriminator} ({x.Mention})").ToArray();
			var botVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "";
			var dotnetVersion = Environment.Version.ToString(3);

			var commitId = ThisAssembly.GitCommitId[..10];
			var commitDate = ThisAssembly.GitCommitDate;
			const string desc =
				"Tantei is bot designed to automatically track and send to Discord its users updates from MyAnimeList, AniList, Shikimori.";

			const string sourceCodeLink = "https://github.com/TanteiBot/Tantei";

			var versions = $"""
								Bot version - {botVersion}
								Commit - {Formatter.MaskedUrl(commitId, new Uri($"{sourceCodeLink}/commit/{commitId.AsSpan(0, 10)}"))}
								Commit date - {commitDate:s}
								DSharpPlus version - {context.Client.VersionString}
								.NET version - {dotnetVersion}
								""";

			var embedBuilder = new DiscordEmbedBuilder
			{
				Title = "About",
				Url = sourceCodeLink,
				Description = desc,
				Color = DiscordColor.DarkBlue,
			}.WithThumbnail(context.Client.CurrentUser.AvatarUrl)
				 .AddField("Links", Formatter.MaskedUrl("Source code", new Uri(sourceCodeLink, UriKind.Absolute)), true)
				 .AddField(owners.Length > 1 ? "Contacts" : "Contact", string.Join('\n', owners), true)
				 .AddField("Versions", versions, true);

			Interlocked.Exchange(ref AboutEmbed, embedBuilder.Build());
		}

		return context.CreateResponseAsync(embed: AboutEmbed);
	}
}