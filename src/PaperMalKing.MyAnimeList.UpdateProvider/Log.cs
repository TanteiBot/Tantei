// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using Microsoft.Extensions.Logging;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.MyAnimeList.UpdateProvider;

internal static partial class Log
{
	[LoggerMessage(LogLevel.Trace, "Didn't find any {UpdateType} updates for {Username}")]
	public static partial void DidntFindAnyFavoritesUpdatesForUser(this ILogger<BaseUpdateProvider> logger, string updateType, string username);

	[LoggerMessage(LogLevel.Trace, "Found {AddedCount} new favorites, {RemovedCount} removed favorites of type {@Type} of {Username}")]
	public static partial void FoundNewFavoritesRemovedFavorites(this ILogger<BaseUpdateProvider> logger, int addedCount, int removedCount, Type type,
																 string username);

	[LoggerMessage(LogLevel.Trace, "Checking favorites updates of {Username}")]
	public static partial void CheckingFavoritesUpdates(this ILogger<BaseUpdateProvider> logger, string username);

	[LoggerMessage(LogLevel.Error, "Encountered unknown error, skipping current update check")]
	public static partial void EncounteredUnknownError(this ILogger<BaseUpdateProvider> logger, Exception exception);

	[LoggerMessage(LogLevel.Error, "Mal server encounters some error, skipping current update check")]
	public static partial void MalServerError(this ILogger<BaseUpdateProvider> logger, Exception exception);

	[LoggerMessage(LogLevel.Warning, "User with username {Username} not found")]
	public static partial void UserNotFound(this ILogger<BaseUpdateProvider> logger, Exception ex, string username);

	[LoggerMessage(LogLevel.Information, "New username for user with {FormerUsername} is {CurrentUsername}")]
	public static partial void NewUsernameForUser(this ILogger<BaseUpdateProvider> logger, string formerUsername, string currentUsername);

	[LoggerMessage(LogLevel.Trace, "Loaded profile for {Username}")]
	public static partial void LoadedProfile(this ILogger<BaseUpdateProvider> logger, string username);

	[LoggerMessage(LogLevel.Debug, "Skipping update check for {Username}, since last update for user was at {Timestamp}")]
	public static partial void SkippingCheckForUser(this ILogger<BaseUpdateProvider> logger, string username, DateTimeOffset timestamp);
}