// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.UpdatesProviders.Base;

public static partial class Log
{
	[LoggerMessage(LogLevel.Information, "Starting checking for updates in {Name} updates provider")]
	internal static partial void StartCheckingForUpdates(this ILogger<BaseUpdateProvider> logger, string name);

	[LoggerMessage(LogLevel.Information, "Stopping {Name} update provider")]
	internal static partial void StopUpdateProvider(this ILogger<BaseUpdateProvider> logger, string name);

	[LoggerMessage(LogLevel.Error, "Exception occured while checking for updates in {Name} updates provider")]
	internal static partial void ErrorOnUpdateCheck(this ILogger<BaseUpdateProvider> logger, Exception ex, string name);

	[LoggerMessage(LogLevel.Information, "Ended checking for updates in {Name} updates provider. Next planned update check is in {DelayBetweenTimerFires}")]
	internal static partial void EndCheckingForUpdates(this ILogger<BaseUpdateProvider> logger, string name, TimeSpan delayBetweenTimerFires);

	[LoggerMessage(LogLevel.Information, "Trying to add {ProviderUsername} {Member} to {Name} update provider")]
	internal static partial void StartAddingUser(this ILogger<BotCommandsModule> logger, string? providerUsername, DiscordMember member, string name);

	[LoggerMessage(LogLevel.Error, "Failed to add {ProviderUsername} {Member} to {Name} update provider")]
	internal static partial void FailAddingUser(this ILogger<BotCommandsModule> logger, Exception ex, string? providerUsername, DiscordMember member,
											  string name);

	[LoggerMessage(LogLevel.Information, "Successfully added {ProviderUsername} {Member} to {Name} update provider")]
	internal static partial void SuccessfullyAddedUser(this ILogger<BotCommandsModule> logger, string? providerUsername, DiscordMember member,
													 string name);

	[LoggerMessage(LogLevel.Information, "Trying to remove {Member} from {Name} update provider")]
	internal static partial void StartRemovingUser(this ILogger<BotCommandsModule> logger, DiscordMember member, string name);

	[LoggerMessage(LogLevel.Error, "Failed to remove {Member} from {Name} update provider")]
	internal static partial void FailRemovingUser(this ILogger<BotCommandsModule> logger, Exception ex, DiscordMember member, string name);

	[LoggerMessage(LogLevel.Information, "Successfully removed {Member} from {Name} update provider")]
	internal static partial void SuccessfullyRemovedUser(this ILogger<BotCommandsModule> logger, DiscordMember member, string name);

	[LoggerMessage(LogLevel.Information, "Trying to enable {Features} feature for {Username}")]
	internal static partial void TryingToEnableFeature(this ILogger<BotCommandsModule> logger, Enum features, string username);

	[LoggerMessage(LogLevel.Error, "Failed to enable {Features} for {Username}")]
	internal static partial void FailedToEnableFeature(this ILogger<BotCommandsModule> logger, Exception ex, Enum features, string username);

	[LoggerMessage(LogLevel.Information, "Successfully enabled {Features} feature for {Username}")]
	internal static partial void SuccessfullyEnabledFeature(this ILogger<BotCommandsModule> logger, Enum features, string username);

	[LoggerMessage(LogLevel.Information, "Trying to disable {Features} feature for {Username}")]
	internal static partial void TryingToDisableFeature(this ILogger<BotCommandsModule> logger, Enum features, string username);

	[LoggerMessage(LogLevel.Error, "Failed to disable {Features} for {Username}")]
	internal static partial void FailedToDisableFeature(this ILogger<BotCommandsModule> logger, Exception ex, Enum features, string username);

	[LoggerMessage(LogLevel.Information, "Successfully disable {Features} feature for {Username}")]
	internal static partial void SuccessfullyDisabledFeature(this ILogger<BotCommandsModule> logger, Enum features, string username);

	[LoggerMessage(LogLevel.Information, "Removing {User}")]
	internal static partial void RemovingUser(this ILogger<GeneralUserService> logger, PaperMalKing.Database.Models.DiscordUser user);

	[LoggerMessage(LogLevel.Information, "Trying to remove user with {UserId} if he has no guilds linked")]
	internal static partial void TryToRemoveUserWithNoGuilds(this ILogger<GeneralUserService> logger, ulong userId);

	[LoggerMessage(LogLevel.Information, "{User} is tracked in some guilds. Skip deleting it")]
	internal static partial void SkipRemovingUserWithGuilds(this ILogger<GeneralUserService> logger, PaperMalKing.Database.Models.DiscordUser user);

	[LoggerMessage(LogLevel.Information, "Removing user with {Id} because he has no guilds linked")]
	internal static partial void RemovingUserWithNoGuilds(this ILogger<GeneralUserService> logger, ulong id);

	[LoggerMessage(LogLevel.Error, "Failed to set color of {UnparsedUpdateType} to {ColorValue}")]
	internal static partial void FailedToSetColor(this ILogger<BotCommandsModule> logger, Exception ex, string unparsedUpdateType, string colorValue);

	[LoggerMessage(LogLevel.Error, "Failed to remove color of {UnparsedUpdateType}")]
	internal static partial void FailedToRemoveColor(this ILogger<BotCommandsModule> logger, Exception ex, string unparsedUpdateType);

	private static readonly Func<ILogger, string?, IDisposable?> AddUserCallback = LoggerMessage.DefineScope<string?>("Adding user with {Username}");

	internal static IDisposable? AddUserScope(this ILogger<BotCommandsModule> logger, string? username) => AddUserCallback(logger, username);

	private static readonly Func<ILogger, string, IDisposable?> RemoveUserCallback = LoggerMessage.DefineScope<string>("Removing user with {Username}");

	internal static IDisposable? RemoveUserScope(this ILogger<BotCommandsModule> logger, string username) => RemoveUserCallback(logger, username);

	private static readonly Func<ILogger, string, string, IDisposable?> RemoveUserInGuildCallback = LoggerMessage.DefineScope<string, string>("Removing user with {Username} in {Guild}");

	internal static IDisposable? RemoveUserInGuildScope(this ILogger<BotCommandsModule> logger, string username, string guildName) => RemoveUserInGuildCallback(logger, username, guildName);

	private static readonly Func<ILogger, string, IDisposable?> CheckingForUsersUpdatesUsernameCallback = LoggerMessage.DefineScope<string>("Checking for users update {Username}");

	private static readonly Func<ILogger, uint, IDisposable?> CheckingForUsersUpdatesIdCallback = LoggerMessage.DefineScope<uint>("Checking for users update {Id}");

	public static IDisposable? CheckingForUsersUpdatesScope(this ILogger<BaseUpdateProvider> logger, string username) => CheckingForUsersUpdatesUsernameCallback(logger, username);

	public static IDisposable? CheckingForUsersUpdatesScope(this ILogger<BaseUpdateProvider> logger, uint id) => CheckingForUsersUpdatesIdCallback(logger, id);
}