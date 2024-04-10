// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using PaperMalKing.Common;
using PaperMalKing.Common.Attributes;

namespace PaperMalKing.Startup.Commands;

[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
[SuppressMessage("Style", """VSTHRD200:Use "Async" suffix for async methods""", Justification = "It doesn't apply to commands")]
[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "It doesn't apply to commands")]
[GuildOnly]
[SlashRequireGuild]
internal sealed class UngroupedCommands : BotCommandsModule
{
	private static DiscordEmbed? _aboutEmbed;

	protected override bool IsResponseVisibleOnlyForRequester => false;

	[SlashCommand("say", "Sends embed in selected channel with selected text")]
	[OwnerOrPermissions(Permissions.ManageGuild)]
	public async Task SayCommand(
		InteractionContext context,
		[Option("channel", "Channel where the embed will be send")] DiscordChannel channelToSayIn,
		[Option("text", "Text to send")] string messageContent)
	{
		if (string.IsNullOrWhiteSpace(messageContent))
		{
			await context.EditResponseAsync(embed: EmbedTemplate.ErrorEmbed("Message's content shouldn't be empty"));
		}

		var embed = new DiscordEmbedBuilder
		{
			Description = messageContent.Replace("@everyone", "@\u200beveryone", StringComparison.Ordinal)
										.Replace("@here", "@\u200bhere", StringComparison.Ordinal),
			Timestamp = TimeProvider.System.GetUtcNow(),
			Color = DiscordColor.Blue,
		}.WithAuthor($"{context.User.Username}#{context.User.Discriminator}", iconUrl: context.User.AvatarUrl);
		try
		{
			await channelToSayIn.SendMessageAsync(embed: embed);
		}
		#pragma warning disable CA1031
		// Modify 'SayCommand' to catch a more specific allowed exception type, or rethrow the exception
		catch
			#pragma warning restore CA1031
		{
			await context.EditResponseAsync(
				new DiscordWebhookBuilder().WithContent("Couldn't send message. Check permissions for bot and try again."));
		}
	}

	[SlashCommand("About", "Displays info about bot")]
	[SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "We format versions in multi-line string")]
	public Task<DiscordMessage> AboutCommand(InteractionContext context)
	{
		if (_aboutEmbed is null)
		{
			var owners = context.Client.CurrentApplication.Owners.Select(x => $"{x.Username} ({x.Mention})").ToArray();
			var botVersion = ThisAssembly.AssemblyVersion.AsSpan(0, 5);
			var dotnetVersion = Environment.Version.ToString(3);

			var commitId = ThisAssembly.GitCommitId[..10];
			var commitDate = new DateTimeOffset(ThisAssembly.GitCommitDate);
			const string desc =
				"Tantei is bot designed to automatically track and send to Discord its users updates from MyAnimeList, AniList, Shikimori.";

			const string sourceCodeLink = "https://github.com/TanteiBot/Tantei";

			var versions = string.Create(CultureInfo.InvariantCulture, $"""
																			Bot version - {botVersion}
																			Commit - {Formatter.MaskedUrl(commitId, new Uri($"{sourceCodeLink}/tree/{commitId}"))}
																			Commit date - {Formatter.Timestamp(commitDate, TimestampFormat.ShortDateTime)}
																			DSharpPlus version - {context.Client.VersionString.AsSpan(0, 14)}
																			.NET version - {dotnetVersion}
																			""");

			var embedBuilder = new DiscordEmbedBuilder
				{
					Title = "About",
					Url = sourceCodeLink,
					Description = desc,
					Color = DiscordColor.DarkBlue,
				}.WithThumbnail(context.Client.CurrentUser.AvatarUrl)
				 .AddField("Links", Formatter.MaskedUrl("Source code", new Uri(sourceCodeLink, UriKind.Absolute)), inline: true)
				 .AddField(owners.Length > 1 ? "Contacts" : "Contact", string.Join('\n', owners), inline: true).AddField("Versions", versions, inline: false);

			Interlocked.Exchange(ref _aboutEmbed, embedBuilder.Build());
		}

		return context.EditResponseAsync(embed: _aboutEmbed);
	}
}