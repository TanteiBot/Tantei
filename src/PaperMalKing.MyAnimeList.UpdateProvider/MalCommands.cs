// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Exceptions;
using PaperMalKing.UpdatesProviders.Base.Features;

namespace PaperMalKing.MyAnimeList.UpdateProvider;

[SlashCommandGroup("mal", "Commands for interacting with MyAnimeList.net")]
[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
[GuildOnly]
[SlashRequireGuild]
[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "It's ok for commands, since they are called externally")]
internal sealed class MalCommands : ApplicationCommandModule
{
	[SlashCommandGroup("user", "Commands for managing user updates from MyAnimeList.net")]
	[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
	public sealed class MalUserCommands : BaseUpdateProviderUserCommandsModule<MalUserService, MalUser>
	{
		public MalUserCommands(MalUserService userService, ILogger<MalUserCommands> logger)
			: base(userService, logger)
		{
		}

		[SlashCommand("add", "Add your MyAnimeList account to being tracked")]
		public override Task AddUserCommand(InteractionContext context, [Option(nameof(username), "Your username on MyAnimeList.net")] string? username = null) =>
			base.AddUserCommand(context, username);

		[SlashCommand("remove", "Remove your MyAnimeList account updates from being tracked")]
		public override Task RemoveUserInGuildCommand(InteractionContext context) => base.RemoveUserInGuildCommand(context);

		[SlashCommand("list", "List accounts of all tracked user's on MyAnimeList in this server")]
		public override Task ListUsersCommand(InteractionContext context) => base.ListUsersCommand(context);

		[SlashCommand("removehere", "Stop sending your updates to this server")]
		public override Task RemoveUserHereCommand(InteractionContext context) => base.RemoveUserHereCommand(context);
	}

	[SlashCommandGroup("features", "Manage your features for updates send from MyAnimeList.net")]
	[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
	public sealed class MalUserFeaturesCommands : BaseUserFeaturesCommandsModule<MalUser, MalUserFeatures>
	{
		public MalUserFeaturesCommands(BaseUserFeaturesService<MalUser, MalUserFeatures> userFeaturesService, ILogger<MalUserFeaturesCommands> logger)
			: base(userFeaturesService, logger)
		{
		}

		[SlashCommand("enable", "Enable features for your updates")]
		public override Task EnableFeatureCommand(
			InteractionContext context,
			[ChoiceProvider(typeof(EnumChoiceProvider<FeaturesChoiceProvider<MalUserFeatures>, MalUserFeatures>)),
			Option("feature", "Feature to enable")]
			string unparsedFeature)
		{
			return base.EnableFeatureCommand(context, unparsedFeature);
		}

		[SlashCommand("disable", "Disable features for your updates")]
		public override Task DisableFeatureCommand(
			InteractionContext context,
			[ChoiceProvider(typeof(EnumChoiceProvider<FeaturesChoiceProvider<MalUserFeatures>, MalUserFeatures>)),
			Option("feature", "Feature to enable")]
			string unparsedFeature) => base.DisableFeatureCommand(context, unparsedFeature);

		[SlashCommand("enabled", "Show features that are enabled for yourself")]
		public override Task EnabledFeaturesCommand(InteractionContext context) => base.EnabledFeaturesCommand(context);

		[SlashCommand("list", "Show all features that are available for updates from MyAnimeList.net")]
		public override Task ListFeaturesCommand(InteractionContext context) => base.ListFeaturesCommand(context);
	}

	[SlashCommandGroup("colors", "Manage colors of your updates")]
	[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
	public sealed class MalColorsCommands : BotCommandsModule
	{
		public ILogger<MalColorsCommands> Logger { get; }

		public MalUserService UserService { get; }

		public MalColorsCommands(ILogger<MalColorsCommands> logger, MalUserService userService)
		{
			this.Logger = logger;
			this.UserService = userService;
		}

		[SlashCommand("set", "Set color for update update")]
		public async Task SetColor(InteractionContext context,
								   [ChoiceProvider(typeof(EnumChoiceProvider<ColorsChoiceProvider<MalUpdateType>, MalUpdateType>)), Option("updateType", "Type of update to set color for")] string unparsedUpdateType,
								   [Option("color", "Color code in hex like #FFFFFF")] string colorValue)
		{
			MalUpdateType updateType;
			try
			{
				var color = new DiscordColor(colorValue);
				updateType = UpdateTypesHelper<MalUpdateType>.Parse(unparsedUpdateType);
				await this.UserService.SetColorAsync(context.User.Id, updateType, color);
			}
			catch (Exception ex)
			{
				var embed = ex is ArgumentException or UserProcessingException ? EmbedTemplate.ErrorEmbed(ex.Message) : EmbedTemplate.UnknownErrorEmbed;
				await context.EditResponseAsync(embed: embed);
				this.Logger.LogError(ex, "Failed to set color of {UnparsedUpdateType} to {ColorValue}", unparsedUpdateType, colorValue);
				throw;
			}

			await context.EditResponseAsync(EmbedTemplate.SuccessEmbed($"Successfully set color of {updateType.ToInvariantString()}"));
		}

		[SlashCommand("remove", "Restore default color for update type")]
		public async Task RemoveColor(InteractionContext context, [ChoiceProvider(typeof(EnumChoiceProvider<ColorsChoiceProvider<MalUpdateType>, MalUpdateType>)), Option("updateType", "Type of update to set color for")] string unparsedUpdateType)
		{
			MalUpdateType updateType;
			try
			{
				updateType = UpdateTypesHelper<MalUpdateType>.Parse(unparsedUpdateType);
				await this.UserService.RemoveColorAsync(context.User.Id, updateType);
			}
			catch (Exception ex)
			{
				var embed = ex is ArgumentException or UserProcessingException ? EmbedTemplate.ErrorEmbed(ex.Message) : EmbedTemplate.UnknownErrorEmbed;
				await context.EditResponseAsync(embed: embed);
				this.Logger.LogError(ex, "Failed to remove color of {UnparsedUpdateType}", unparsedUpdateType);
				throw;
			}

			await context.EditResponseAsync(EmbedTemplate.SuccessEmbed($"Successfully removed color of {updateType.ToInvariantString()}"));
		}

		[SlashCommand("list", "Lists your overriden types")]
		public Task<DiscordMessage> ListOverridenColor(InteractionContext context)
		{
			var colors = this.UserService.OverridenColors(context.User.Id);
			return context.EditResponseAsync(EmbedTemplate.SuccessEmbed(string.IsNullOrWhiteSpace(colors) ? "You have no colors overriden" : "Your overriden colors").WithDescription(colors));
		}
	}
}