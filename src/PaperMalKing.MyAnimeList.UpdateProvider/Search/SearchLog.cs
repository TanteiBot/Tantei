// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.Extensions.Logging;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal static partial class SearchLog
{
	[LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "MyAnimeList search was denied by the channel permission pre-check")]
	public static partial void PermissionDenied(this ILogger logger);

	[LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "MyAnimeList official API rejected the search with 403")]
	public static partial void OfficialApiForbidden(this ILogger logger);

	[LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "MyAnimeList search was rejected by a full rate limiter queue")]
	public static partial void RateLimiterQueueRejected(this ILogger logger);

	[LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "MyAnimeList search failed")]
	public static partial void SearchFailed(this ILogger logger, Exception exception);

	[LoggerMessage(EventId = 5, Level = LogLevel.Warning, Message = "Discord rejected the public MyAnimeList search post with 403")]
	public static partial void PublicPostForbidden(this ILogger logger);

	[LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "Posting the MyAnimeList search result publicly failed")]
	public static partial void PublicPostFailed(this ILogger logger, Exception exception);

	[LoggerMessage(EventId = 7, Level = LogLevel.Error, Message = "Handling a MyAnimeList Picker interaction failed unexpectedly")]
	public static partial void PickerInteractionFailed(this ILogger logger, Exception exception);

	[LoggerMessage(EventId = 8, Level = LogLevel.Warning, Message = "Pushing the terminal state of a MyAnimeList Picker failed")]
	public static partial void TerminalStatePushFailed(this ILogger logger, Exception exception);

	[LoggerMessage(
		EventId = 9,
		Level = LogLevel.Information,
		Message = "MyAnimeList search completed with {Outcome} over {FloorSurvivorCount} floor survivors and {ResultCount} results")]
	public static partial void SearchCompleted(this ILogger logger, SearchOutcomeKind outcome, int floorSurvivorCount, int resultCount);

	[LoggerMessage(EventId = 10, Level = LogLevel.Information, Message = "MyAnimeList Picker ended with {Outcome}, media {SelectedMediaId}")]
	public static partial void PickerEnded(this ILogger logger, PickerTerminalReason outcome, uint? selectedMediaId);

	[LoggerMessage(EventId = 11, Level = LogLevel.Information, Message = "MyAnimeList Picker session is no longer available")]
	public static partial void PickerUnavailable(this ILogger logger);

	public static IDisposable? SearchScope(this ILogger logger, Guid searchId, PickerSearchContext context) =>
		logger.BeginScope(SearchLogScope.ForSearch(searchId, context));

	public static IDisposable? PickerInteractionScope(
		this ILogger logger,
		Guid searchId,
		ulong discordUserId,
		string discordDisplayName,
		ulong? guildId,
		ulong? channelId) =>
		logger.BeginScope(SearchLogScope.ForInteraction(searchId, discordUserId, discordDisplayName, guildId, channelId));
}
