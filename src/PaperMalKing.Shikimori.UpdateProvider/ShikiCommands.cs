// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Colors;
using PaperMalKing.UpdatesProviders.Base.Features;

namespace PaperMalKing.Shikimori.UpdateProvider;

[SlashCommandGroup("shiki", "Commands for interacting with Shikimori.one")]
[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
[GuildOnly]
[SlashRequireGuild]
internal sealed class ShikiCommands : ApplicationCommandModule
{
	[SlashCommandGroup("user", "Commands for managing user updates from Shikimori.one")]
	[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
	public sealed class ShikiUserCommands : BaseUpdateProviderUserCommandsModule<ShikiUserService, ShikiUser>
	{
		public ShikiUserCommands(ShikiUserService userService, ILogger<ShikiUserCommands> logger)
			: base(userService, logger)
		{
		}

		[SlashCommand("add", "Add your Shikimori account to being tracked")]
		public override Task AddUserCommand(InteractionContext context, [Option(nameof(username), "Your username on Shikimori")] string? username = null) =>
			base.AddUserCommand(context, username);

		[SlashCommand("remove", "Remove your Shikimori account updates from being tracked")]
		public override Task RemoveUserInGuildCommand(InteractionContext context) => base.RemoveUserInGuildCommand(context);

		[SlashCommand("list", "List accounts of all tracked user's on Shikimori in this server")]
		public override Task ListUsersCommand(InteractionContext context) => base.ListUsersCommand(context);

		[SlashCommand("removehere", "Stop sending your updates to this server")]
		public override Task RemoveUserHereCommand(InteractionContext context) => base.RemoveUserHereCommand(context);
	}

	[SlashCommandGroup("features", "Manage your features for updates send from Shikimori.one")]
	[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
	public sealed class ShikiUserFeaturesCommands : BaseUserFeaturesCommandsModule<ShikiUser, ShikiUserFeatures>
	{
		public ShikiUserFeaturesCommands(BaseUserFeaturesService<ShikiUser, ShikiUserFeatures> userFeaturesService, ILogger<ShikiUserFeaturesCommands> logger)
			: base(userFeaturesService, logger)
		{
		}

		[SlashCommand("enable", "Enable features for your updates")]
		public override Task EnableFeatureCommand(
								InteractionContext context,
								[ChoiceProvider(typeof(EnumChoiceProvider<FeaturesChoiceProvider<ShikiUserFeatures>, ShikiUserFeatures>)),
								Option("feature", "Feature to enable")]
								string unparsedFeature) => base.EnableFeatureCommand(context, unparsedFeature);

		[SlashCommand("disable", "Disable features for your updates")]
		public override Task DisableFeatureCommand(
								InteractionContext context,
								[ChoiceProvider(typeof(EnumChoiceProvider<FeaturesChoiceProvider<ShikiUserFeatures>, ShikiUserFeatures>)),
								Option("feature", "Feature to disable")]
								string unparsedFeature) => base.DisableFeatureCommand(context, unparsedFeature);

		[SlashCommand("enabled", "Show features that are enabled for yourself")]
		public override Task EnabledFeaturesCommand(InteractionContext context) => base.EnabledFeaturesCommand(context);

		[SlashCommand("list", "Show all features that are available for updates from Shikimori.one")]
		public override Task ListFeaturesCommand(InteractionContext context) => base.ListFeaturesCommand(context);
	}

	[SlashCommandGroup("colors", "Manage colors of your updates")]
	[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
	public sealed class ShikiColorsCommands : BaseColorsCommandModule<ShikiUser, ShikiUpdateType>
	{
		public ShikiColorsCommands(ILogger<ShikiColorsCommands> logger, CustomColorService<ShikiUser, ShikiUpdateType> colorService)
			: base(logger, colorService)
		{
		}

		[SlashCommand("set", "Set color for update update")]
		public override Task SetColor(InteractionContext context,
								   [ChoiceProvider(typeof(EnumChoiceProvider<ColorsChoiceProvider<ShikiUpdateType>, ShikiUpdateType>)), Option("updateType", "Type of update to set color for")] string unparsedUpdateType,
								   [Option("color", "Color code in hex like #FFFFFF")] string colorValue) => base.SetColor(context, unparsedUpdateType, colorValue);

		[SlashCommand("remove", "Restore default color for update type")]
		public override Task RemoveColor(InteractionContext context, [ChoiceProvider(typeof(EnumChoiceProvider<ColorsChoiceProvider<ShikiUpdateType>, ShikiUpdateType>)), Option("updateType", "Type of update to set color for")] string unparsedUpdateType) => base.RemoveColor(context, unparsedUpdateType);

		[SlashCommand("list", "Lists your overriden types")]
		public override Task<DiscordMessage> ListOverridenColor(InteractionContext context) => base.ListOverridenColor(context);
	}
}