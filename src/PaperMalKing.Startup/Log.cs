// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using PaperMalKing.Startup.Data;
using PaperMalKing.Startup.Services;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.Startup;

internal static partial class Log
{
	[LoggerMessage(LogLevel.Trace, "Building {@UpdateProvidersConfigurationService}")]
	public static partial void BuildingUpdateProvidersConfigurationService(this ILogger<UpdateProvidersConfigurationService> logger,
																		   Type updateProvidersConfigurationService);

	[LoggerMessage(LogLevel.Trace, "Built {@UpdateProvidersConfigurationService}")]
	public static partial void BuiltUpdateProvidersConfigurationService(this ILogger<UpdateProvidersConfigurationService> logger,
																		Type updateProvidersConfigurationService);

	[LoggerMessage(LogLevel.Debug, "Registering {@UpdateProvider} update provider")]
	public static partial void RegisteringUpdateProvider(this ILogger<UpdateProvidersConfigurationService> logger, IUpdateProvider updateProvider);

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
	public static partial void RemovingChannel(this ILogger<GuildManagementService> logger, ulong id);
}