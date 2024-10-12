// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Reflection;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using PaperMalKing.Startup.Data;
using PaperMalKing.Startup.Services;
using PaperMalKing.Startup.Services.Background;

namespace PaperMalKing.Startup;

internal static partial class Log
{
	[LoggerMessage(LogLevel.Trace, "Building {@UpdateProvidersConfigurationService}")]
	public static partial void BuildingUpdateProvidersConfigurationService(this ILogger<UpdateProvidersConfigurationService> logger,
																		   Type updateProvidersConfigurationService);

	[LoggerMessage(LogLevel.Trace, "Built {@UpdateProvidersConfigurationService}")]
	public static partial void BuiltUpdateProvidersConfigurationService(this ILogger<UpdateProvidersConfigurationService> logger,
																		Type updateProvidersConfigurationService);

	[LoggerMessage(LogLevel.Debug, "Registering {UpdateProvider} update provider")]
	public static partial void RegisteringUpdateProvider(this ILogger<UpdateProvidersConfigurationService> logger, string updateProvider);

	[LoggerMessage(LogLevel.Critical, "No update providers were registered")]
	public static partial void NoUpdateProvidersRegistered(this ILogger<UpdateProvidersConfigurationService> logger);

	[LoggerMessage(LogLevel.Information, "Starting to look for users without links to any guild, or users that left server while bot was offline")]
	public static partial void StartingUserCleanup(this ILogger<UserCleanupService> logger);

	[LoggerMessage(LogLevel.Information, "Finishing looking for users without links to any guild")]
	public static partial void FinishingUserCleanup(this ILogger<UserCleanupService> logger);

	[LoggerMessage(LogLevel.Warning, "Posting update to {Channel} - {@Embed}")]
	public static partial void PostingUpdate(this ILogger<UpdatePoster> logger, DiscordChannel channel, DiscordEmbed embed);

	[LoggerMessage(LogLevel.Debug, "{Command} was successfully executed by request of {Member}")]
	public static partial void CommandSuccessfullyExecuted(this ILogger<CommandsService> logger, string command, DiscordMember member);

	[LoggerMessage(LogLevel.Error, "{Command} errored with exception while trying to be executed by {Member}")]
	public static partial void CommandErrored(this ILogger<CommandsService> logger, Exception ex, string command, DiscordMember member);

	[LoggerMessage(LogLevel.Information, "Updating channel of {Guild} from {CurrentChannelId} to {NewChannelId}")]
	public static partial void UpdatingChannel(this ILogger<GuildManagementService> logger, DiscordGuild guild, ulong currentChannelId,
											   ulong newChannelId);

	[LoggerMessage(LogLevel.Information, "Setting channel for guild {Guild} at {Channel}")]
	public static partial void SettingChannelForGuild(this ILogger<GuildManagementService> logger, DiscordGuild guild, DiscordChannel channel);

	[LoggerMessage(LogLevel.Information, "Removing guild with {Id}")]
	public static partial void RemovingGuild(this ILogger<GuildManagementService> logger, ulong id);

	[LoggerMessage(LogLevel.Information, "Guild {Guild} became unavailable")]
	public static partial void GuildBecameUnavailable(this ILogger<DiscordBackgroundService> logger, DiscordGuild guild);

	[LoggerMessage(LogLevel.Information, "Bot was removed from {Guild}")]
	public static partial void BotWasRemovedFromGuild(this ILogger<DiscordBackgroundService> logger, DiscordGuild guild);

	[LoggerMessage(LogLevel.Critical, "Task on removing guild from db faulted")]
	public static partial void RemovingGuildFromDbFailed(this ILogger<DiscordBackgroundService> logger, Exception? exception);

	[LoggerMessage(LogLevel.Information, "Discord client resumed")]
	public static partial void DiscordClientResumed(this ILogger<DiscordBackgroundService> logger);

	[LoggerMessage(LogLevel.Information, "Discord client ready")]
	public static partial void DiscordClientReady(this ILogger<DiscordBackgroundService> logger);

	[LoggerMessage(LogLevel.Error, "Discord client errored")]
	public static partial void DiscordClientErrored(this ILogger<DiscordBackgroundService> logger, Exception? exception);

	[LoggerMessage(LogLevel.Debug, "User {Member} left guild {Guild}")]
	public static partial void UserLeftGuild(this ILogger<DiscordBackgroundService> logger, DiscordMember member, DiscordGuild guild);

	[LoggerMessage(LogLevel.Debug, "User {Member} that left wasn't saved in db")]
	public static partial void UserThatLeftWasNotInDb(this ILogger<DiscordBackgroundService> logger, DiscordMember member);

	[LoggerMessage(LogLevel.Error, "Task on removing left member from the guild failed due to unknown reason")]
	public static partial void TaskOnRemovingUserFailed(this ILogger<DiscordBackgroundService> logger, Exception? exception);

	[LoggerMessage(LogLevel.Information, "Connecting to Discord")]
	public static partial void ConnectingToDiscord(this ILogger<DiscordBackgroundService> logger);

	[LoggerMessage(LogLevel.Information, "Disconnecting from Discord")]
	public static partial void DisconnectingFromDiscord(this ILogger<DiscordBackgroundService> logger);

	[LoggerMessage(LogLevel.Error, "Error occured while updating Discord presence")]
	public static partial void ErrorOccuredWhileChangingDiscordPresence(this ILogger<DiscordBackgroundService> logger, Exception? exception);

	[LoggerMessage(LogLevel.Information, "Found only one Discord status in options so it won't be changed")]
	public static partial void NoStatusWouldBeChanged(this ILogger<DiscordBackgroundService> logger);

	[LoggerMessage(LogLevel.Error, "Couldn't parse correct ActivityType from {ActivityType}, correct values are {CorrectActivities}")]
	public static partial void CouldNotParseCorrectActivity(this ILogger<DiscordBackgroundService> logger, string activityType, string correctActivities);

	[LoggerMessage(LogLevel.Error, "Couldn't parse correct UserStatus from {Status}, correct values are {CorrectStatuses}")]
	public static partial void CouldNotParseCorrectStatus(this ILogger<DiscordBackgroundService> logger, string status, string correctStatuses);

	[LoggerMessage(LogLevel.Trace, "Found {Assembly} which may contain Commands modules")]
	public static partial void FoundAssemblyWhichMayContainCommands(this ILogger<CommandsService> logger, Assembly assembly);

	[LoggerMessage(LogLevel.Trace, "Trying to register {@Type} command module")]
	public static partial void TryingToRegisterTypeAsCommandModule(this ILogger<CommandsService> logger, Type type);

	[LoggerMessage(LogLevel.Error, "Error occured while trying to register {@Type}")]
	public static partial void ErrorOccuredWhileTryingToRegisterCommandModule(this ILogger<CommandsService> logger, Exception ex, Type type);

	[LoggerMessage(LogLevel.Debug, "Successfully registered {@Type}")]
	public static partial void SuccessfullyRegisteredType(this ILogger<CommandsService> logger, Type type);

	[LoggerMessage(LogLevel.Trace, "Building Commands service finished")]
	public static partial void BuildingCommandsServiceFinished(this ILogger<CommandsService> logger);

	[LoggerMessage(LogLevel.Debug, "Starting querying posting channels")]
	public static partial void StartingQueryingPostingChannels(this ILogger<UpdatePublishingService> logger);

	[LoggerMessage(LogLevel.Debug, "Ended querying posting channels")]
	public static partial void EndedQueryingPostingChannels(this ILogger<UpdatePublishingService> logger);

	[LoggerMessage(LogLevel.Debug, "Trying to get guild with {Id}")]
	public static partial void TryingToGetGuildWithId(this ILogger<UpdatePublishingService> logger, ulong id);

	[LoggerMessage(LogLevel.Trace, "Loaded guild {Guild}")]
	public static partial void LoadedGuild(this ILogger<UpdatePublishingService> logger, DiscordGuild? guild);

	[LoggerMessage(LogLevel.Trace, "Loaded channel {Channel} in guild {DiscordGuild}")]
	public static partial void LoadedChannelInGuild(this ILogger<UpdatePublishingService> logger, DiscordChannel? channel, DiscordGuild? discordGuild);

	[LoggerMessage(LogLevel.Error, "Task on loading channels to post to failed")]
	public static partial void LoadingChannelsToPostFailed(this ILogger<UpdatePublishingService> logger, Exception? ex);
}