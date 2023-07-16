// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database.Models.AniList;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Features;

namespace PaperMalKing.AniList.UpdateProvider;

[SlashCommandGroup("anilist", "Commands for interacting with AniList.co")]
[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
[GuildOnly, SlashRequireGuild]
internal sealed class AniListCommands : ApplicationCommandModule
{
	[SlashCommandGroup("user", "Commands for managing user updates from AniList.co")]
	[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
	public sealed class AniListUserCommands : BaseUpdateProviderUserCommandsModule<AniListUserService, AniListUser>
	{
		public AniListUserCommands(AniListUserService userService, ILogger<AniListUserCommands> logger) : base(userService, logger)
		{ }

		[SlashCommand("add", "Add your AniList account to being tracked")]
		public override Task AddUserCommand(InteractionContext context,
											[Option(nameof(username), "Your username on AniList")] string? username = null) =>
			base.AddUserCommand(context, username);

		[SlashCommand("remove", "Remove your AniList account updates from being tracked")]
		public override Task RemoveUserInGuildCommand(InteractionContext context) => base.RemoveUserInGuildCommand(context);

		[SlashCommand("list", "List accounts of all tracked users on AniList in this server")]
		public override Task ListUsersCommand(InteractionContext context) => base.ListUsersCommand(context);

		[SlashCommand("removehere", "Stop sending your updates to this server")]
		public override Task RemoveUserHereCommand(InteractionContext context) => base.RemoveUserHereCommand(context);
	}

	[SlashCommandGroup("features", "Manage your features for updates send from AniList.co")]
	[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
	public sealed class AniListUserFeaturesCommands : BaseUserFeaturesCommandsModule<AniListUser, AniListUserFeatures>
	{
		public AniListUserFeaturesCommands(BaseUserFeaturesService<AniListUser, AniListUserFeatures> userFeaturesService,
										   ILogger<AniListUserFeaturesCommands> logger) : base(userFeaturesService, logger)
		{ }

		[SlashCommand("enable", "Enable features for your updates")]
		public override Task EnableFeatureCommand(InteractionContext context,
												  [ChoiceProvider(typeof(FeaturesChoiceProvider<AniListUserFeatures>)),
												   Option("feature", "Feature to enable")]
												  string unparsedFeature) => base.EnableFeatureCommand(context, unparsedFeature);

		[SlashCommand("disable", "Disable features for your updates")]
		public override Task DisableFeatureCommand(InteractionContext context,
												   [ChoiceProvider(typeof(FeaturesChoiceProvider<AniListUserFeatures>)),
												    Option("feature", "Feature to enable")]
												   string unparsedFeature) => base.DisableFeatureCommand(context, unparsedFeature);

		[SlashCommand("enabled", "Show features that are enabled for yourself")]
		public override Task EnabledFeaturesCommand(InteractionContext context) => base.EnabledFeaturesCommand(context);

		[SlashCommand("list", "Show all features that are available for updates from AniList.co")]
		public override Task ListFeaturesCommand(InteractionContext context) => base.ListFeaturesCommand(context);
	}
}