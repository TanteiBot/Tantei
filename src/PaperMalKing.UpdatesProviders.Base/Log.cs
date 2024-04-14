// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.UpdatesProviders.Base;

internal static partial class Log
{
	[LoggerMessage(LogLevel.Information, "Starting checking for updates in {Name} updates provider")]
	public static partial void StartCheckingForUpdates(this ILogger<BaseUpdateProvider> logger, string name);

	[LoggerMessage(LogLevel.Information, "Stopping {Name} update provider")]
	public static partial void StopUpdateProvider(this ILogger<BaseUpdateProvider> logger, string name);

	[LoggerMessage(LogLevel.Error, "Exception occured while checking for updates in {Name} updates provider")]
	public static partial void ErrorOnUpdateCheck(this ILogger<BaseUpdateProvider> logger, Exception ex, string name);

	[LoggerMessage(LogLevel.Information,
		"Ended checking for updates in {Name} updates provider. Next planned update check is in {@DelayBetweenTimerFires}")]
	public static partial void EndCheckingForUpdates(this ILogger<BaseUpdateProvider> logger, string name, TimeSpan delayBetweenTimerFires);

	[LoggerMessage(LogLevel.Information, "Trying to add {ProviderUsername} {Member} to {Name} update provider")]
	public static partial void StartAddingUser(this ILogger<BotCommandsModule> logger, string? providerUsername, DiscordMember member, string name);

	[LoggerMessage(LogLevel.Error, "Failed to add {ProviderUsername} {Member} to {Name} update provider")]
	public static partial void FailAddingUser(this ILogger<BotCommandsModule> logger, Exception ex, string? providerUsername, DiscordMember member,
											  string name);

	[LoggerMessage(LogLevel.Information, "Successfully added {ProviderUsername} {Member} to {Name} update provider")]
	public static partial void SuccessfullyAddedUser(this ILogger<BotCommandsModule> logger, string? providerUsername, DiscordMember member,
													 string name);

	[LoggerMessage(LogLevel.Information, "Trying to remove {Member} from {Name} update provider")]
	public static partial void StartRemovingUser(this ILogger<BotCommandsModule> logger, DiscordMember member, string name);

	[LoggerMessage(LogLevel.Error, "Failed to remove {Member} from {Name} update provider")]
	public static partial void FailRemovingUser(this ILogger<BotCommandsModule> logger, Exception ex, DiscordMember member, string name);

	[LoggerMessage(LogLevel.Information, "Successfully removed {Member} from {Name} update provider")]
	public static partial void SuccessfullyRemovedUser(this ILogger<BotCommandsModule> logger, DiscordMember member, string name);

	[LoggerMessage(LogLevel.Information, "Trying to enable {Features} feature for {Username}")]
	public static partial void TryingToEnableFeature(this ILogger<BotCommandsModule> logger, Enum features, string username);

	[LoggerMessage(LogLevel.Error, "Failed to enable {Features} for {Username}")]
	public static partial void FailedToEnableFeature(this ILogger<BotCommandsModule> logger, Exception ex, Enum features, string username);

	[LoggerMessage(LogLevel.Information, "Successfully enabled {Features} feature for {Username}")]
	public static partial void SuccessfullyEnabledFeature(this ILogger<BotCommandsModule> logger, Enum features, string username);

	[LoggerMessage(LogLevel.Information, "Trying to disable {Features} feature for {Username}")]
	public static partial void TryingToDisableFeature(this ILogger<BotCommandsModule> logger, Enum features, string username);

	[LoggerMessage(LogLevel.Error, "Failed to disable {Features} for {Username}")]
	public static partial void FailedToDisableFeature(this ILogger<BotCommandsModule> logger, Exception ex, Enum features, string username);

	[LoggerMessage(LogLevel.Information, "Successfully disable {Features} feature for {Username}")]
	public static partial void SuccessfullyDisabledFeature(this ILogger<BotCommandsModule> logger, Enum features, string username);

	[LoggerMessage(LogLevel.Information, "Removing {User}")]
	public static partial void RemovingUser(this ILogger<GeneralUserService> logger, PaperMalKing.Database.Models.DiscordUser user);

	[LoggerMessage(LogLevel.Information, "Trying to remove user with {UserId} if he has no guilds linked")]
	public static partial void TryToRemoveUserWithNoGuilds(this ILogger<GeneralUserService> logger, ulong userId);

	[LoggerMessage(LogLevel.Information, "{User} is tracked in some guilds. Skip deleting it")]
	public static partial void SkipRemovingUserWithGuilds(this ILogger<GeneralUserService> logger, PaperMalKing.Database.Models.DiscordUser user);

	[LoggerMessage(LogLevel.Information, "Removing user with {Id} because he has no guilds linked")]
	public static partial void RemovingUserWithNoGuilds(this ILogger<GeneralUserService> logger, ulong id);

	[LoggerMessage(LogLevel.Error, "Failed to set color of {UnparsedUpdateType} to {ColorValue}")]
	public static partial void FailedToSetColor(this ILogger<BotCommandsModule> logger, Exception ex, string unparsedUpdateType, string colorValue);

	[LoggerMessage(LogLevel.Error, "Failed to remove color of {UnparsedUpdateType}")]
	public static partial void FailedToRemoveColor(this ILogger<BotCommandsModule> logger, Exception ex, string unparsedUpdateType);
}