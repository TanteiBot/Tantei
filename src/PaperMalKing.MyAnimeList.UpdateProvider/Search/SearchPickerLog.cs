// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.Extensions.Logging;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal static partial class SearchPickerLog
{
	[LoggerMessage(LogLevel.Error, "Unexpected failure while handling MyAnimeList Picker {SearchId}")]
	public static partial void UnexpectedInteractionFailure(this ILogger logger, Exception exception, string searchId);

	[LoggerMessage(LogLevel.Warning, "Failed to push terminal state for MyAnimeList Picker {SearchId}")]
	public static partial void TerminalStatePushFailed(this ILogger logger, Exception exception, string searchId);

	[LoggerMessage(LogLevel.Error, "Failed to post selection for MyAnimeList Picker {SearchId}")]
	public static partial void SelectionPostFailed(this ILogger logger, Exception exception, string searchId);
}
