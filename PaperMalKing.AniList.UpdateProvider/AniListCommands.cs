// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database.Models.AniList;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Features;

namespace PaperMalKing.AniList.UpdateProvider;

[SlashCommandGroup("anilist", "Commands for interacting with AniList.co")]
[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
public sealed class AniListCommands : ApplicationCommandModule
{
	[SlashCommandGroup("user", "Commands for managing user updates from AniList.co", true)]
	[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
	public sealed class AniListUserCommands : BaseUpdateProviderUserCommandsModule<AniListUserService>
	{
		public AniListUserCommands(AniListUserService userService, ILogger<AniListUserCommands> logger) : base(userService, logger)
		{ }

		[SlashCommand("add", "Add your AniList account to being tracked", true)]
		public override Task AddUserCommand(InteractionContext ctx, [Option(nameof(username), "Your username on AniList")] string username) =>
			base.AddUserCommand(ctx, username);

		[SlashCommand("remove", "Remove your AniList account updates from being tracked", true)]
		public override Task RemoveUserInGuildCommand(InteractionContext ctx) => base.RemoveUserInGuildCommand(ctx);

		[SlashCommand("list", "List accounts of all tracked users on AniList in this server")]
		public override Task ListUsersCommand(InteractionContext ctx) => base.ListUsersCommand(ctx);

		[SlashCommand("removehere", "Stop sending your updates to this server", true)]
		public override Task RemoveUserHereCommand(InteractionContext ctx) => base.RemoveUserHereCommand(ctx);
	}

	[SlashCommandGroup("features", "Manage your features for updates send from AniList.co", true)]
	[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
	public sealed class ShikiUserFeaturesCommands : BaseUserFeaturesCommandsModule<AniListUserFeatures>
	{
		public ShikiUserFeaturesCommands(IUserFeaturesService<AniListUserFeatures> userFeaturesService, ILogger<ShikiUserFeaturesCommands> logger) :
			base(userFeaturesService, logger)
		{ }

		[SlashCommand("enable", "Enable features for your updates", true)]
		public override Task EnableFeatureCommand(InteractionContext context,
												  [ChoiceProvider(typeof(FeaturesChoiceProvider<AniListUserFeatures>)),
												   Option("feature", "Feature to enable")]
												  string unparsedFeature) => base.EnableFeatureCommand(context, unparsedFeature);

		[SlashCommand("disable", "Disable features for your updates", true)]
		public override Task DisableFeatureCommand(InteractionContext context,
												   [ChoiceProvider(typeof(FeaturesChoiceProvider<AniListUserFeatures>)),
												    Option("feature", "Feature to enable")]
												   string unparsedFeature) => base.DisableFeatureCommand(context, unparsedFeature);

		[SlashCommand("enabled", "Show features that are enabled for yourself", true)]
		public override Task EnabledFeaturesCommand(InteractionContext context) => base.EnabledFeaturesCommand(context);

		[SlashCommand("list", "Show all features that are available for updates from AniList.co", true)]
		public override Task ListFeaturesCommand(InteractionContext context) => base.ListFeaturesCommand(context);
	}
}