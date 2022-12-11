// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Features;

namespace PaperMalKing.UpdatesProviders.MyAnimeList;

[SlashCommandGroup("mal", "Commands for managing user updates from MyAnimeList.net", true)]
[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
public sealed class MalCommands : BaseUpdateProviderUserCommandsModule<MalUserService>
{
	/// <inheritdoc />
	public MalCommands(MalUserService userService, ILogger<MalCommands> logger) : base(userService, logger)
	{ }

	[SlashCommand("add", "Add your MyAnimeList account to being tracked", true)]
	public override Task AddUserCommand(InteractionContext ctx, [Option("username", "Your username on MyAnimeList.net")] string username) =>
		base.AddUserCommand(ctx, username);

	[SlashCommand("remove", "Remove your MyAnimeList account updates from being tracked", true)]
	public override Task RemoveUserInGuildCommand(InteractionContext ctx) => base.RemoveUserInGuildCommand(ctx);

	[SlashCommand("list", "List accounts of all tracked user's on MyAnimeList in this server", true)]
	public override Task ListUsersCommand(InteractionContext ctx) => base.ListUsersCommand(ctx);

	[SlashCommand("removehere", "Stop sending your updates to this server", true)]
	public override Task RemoveUserHereCommand(InteractionContext ctx) => base.RemoveUserHereCommand(ctx);
}
#pragma warning disable CA1034
[SlashCommandGroup("malfeatures", "Manage your features for updates send from MyAnimeList.net", true)]
[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
public sealed class MalUserFeaturesCommands : BaseUserFeaturesCommandsModule<MalUserFeatures>
{
	public MalUserFeaturesCommands(IUserFeaturesService<MalUserFeatures> userFeaturesService, ILogger<MalUserFeaturesCommands> logger) : base(
		userFeaturesService, logger)
	{ }

	[SlashCommand("enable", "Enable features for your updates", true)]
	public override Task EnableFeatureCommand(InteractionContext context,
											  [ChoiceProvider(typeof(FeaturesChoiceProvider<MalUserFeatures>)),
											   Option("feature", "Feature to enable")]
											  string unparsedFeature)
	{
		return base.EnableFeatureCommand(context, unparsedFeature);
	}

	[SlashCommand("disable", "Disable features for your updates", true)]
	public override Task DisableFeatureCommand(InteractionContext context,
											   [ChoiceProvider(typeof(FeaturesChoiceProvider<MalUserFeatures>)),
											    Option("feature", "Feature to enable")]
											   string unparsedFeature) => base.DisableFeatureCommand(context, unparsedFeature);

	[SlashCommand("enabled", "Show features that are enabled for yourself", true)]
	public override Task EnabledFeaturesCommand(InteractionContext context) => base.EnabledFeaturesCommand(context);

	[SlashCommand("list", "Show all features that are available for updates from MyAnimeList.net", true)]
	public override Task ListFeaturesCommand(InteractionContext context) => base.ListFeaturesCommand(context);
}