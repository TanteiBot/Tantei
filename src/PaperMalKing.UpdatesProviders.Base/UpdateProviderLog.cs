// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using Microsoft.Extensions.Logging;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.UpdatesProviders.Base;

public static partial class UpdateProviderLog
{
	[LoggerMessage(LogLevel.Debug, "Starting to check for updates of {UserId}")]
	public static partial void StartingToCheckUpdatesFor(this ILogger<BaseUpdateProvider> logger, uint userId);

	[LoggerMessage(LogLevel.Debug, "Starting to check for updates of {Username}")]
	public static partial void StartingToCheckUpdatesFor(this ILogger<BaseUpdateProvider> logger, string username);

	[LoggerMessage(LogLevel.Trace, "No updates found for {Username}")]
	public static partial void NoUpdatesFound(this ILogger<BaseUpdateProvider> logger, string? username);

	[LoggerMessage(LogLevel.Trace, "No updates found for {@Id}")]
	public static partial void NoUpdatesFound(this ILogger<BaseUpdateProvider> logger, uint id);

	[LoggerMessage(LogLevel.Information, "Cancellation requested. Stopping")]
	public static partial void CancellationRequested(this ILogger<BaseUpdateProvider> logger);

	[LoggerMessage(LogLevel.Error, "Error happened while sending update or saving changes to DB")]
	public static partial void ErrorHappenedWhileSendingUpdateOrSavingToDb(this ILogger<BaseUpdateProvider> logger, Exception ex);

	[LoggerMessage(LogLevel.Debug, "Found {Count} updates for {Username}")]
	public static partial void FoundUpdatesForUser(this ILogger<BaseUpdateProvider> logger, int count, string username);
}