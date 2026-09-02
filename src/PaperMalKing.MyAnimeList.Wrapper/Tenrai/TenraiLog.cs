// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.Extensions.Logging;

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

internal static partial class TenraiLog
{
	[LoggerMessage(
		EventId = 1,
		Level = LogLevel.Warning,
		Message =
			"Tenrai {Operation} enrichment for MAL id {MediaId} failed terminally ({Kind}); status {Status}, {RetryCount} retries, {ElapsedMilliseconds} ms, Retry-After {RetryAfter}")]
	public static partial void TenraiEnrichmentFailed(
		this ILogger logger,
		string operation,
		long mediaId,
		TenraiFailureKind kind,
		int? status,
		int retryCount,
		long elapsedMilliseconds,
		TimeSpan? retryAfter);

	[LoggerMessage(EventId = 2, Level = LogLevel.Debug, Message = "Tenrai {Operation} enrichment for MAL id {MediaId} returned 404")]
	public static partial void TenraiEnrichmentNotFound(this ILogger logger, string operation, long mediaId);

	[LoggerMessage(
		EventId = 3,
		Level = LogLevel.Debug,
		Message = "Tenrai {Operation} enrichment for MAL id {MediaId} skipped while the circuit is open")]
	public static partial void TenraiEnrichmentCircuitSkipped(this ILogger logger, string operation, long mediaId);

	[LoggerMessage(
		EventId = 4,
		Level = LogLevel.Debug,
		Message = "Tenrai {Operation} enrichment for MAL id {MediaId} suppressed by the active Retry-After cooldown")]
	public static partial void TenraiEnrichmentCooldownSuppressed(this ILogger logger, string operation, long mediaId);

	[LoggerMessage(
		EventId = 5,
		Level = LogLevel.Debug,
		Message = "Tenrai {Operation} enrichment for MAL id {MediaId} shed by a full request queue")]
	public static partial void TenraiEnrichmentQueueRejected(this ILogger logger, string operation, long mediaId);

	[LoggerMessage(
		EventId = 6,
		Level = LogLevel.Warning,
		Message = "Tenrai enrichment circuit opened for {OpenDurationSeconds} s after repeated terminal failures")]
	public static partial void TenraiCircuitOpened(this ILogger logger, double openDurationSeconds);

	[LoggerMessage(EventId = 7, Level = LogLevel.Warning, Message = "Tenrai enrichment circuit closed and resumed calling the provider")]
	public static partial void TenraiCircuitClosed(this ILogger logger);

	[LoggerMessage(EventId = 8, Level = LogLevel.Debug, Message = "Requesting {Id} anime details")]
	public static partial void RequestingAnimeDetails(this ILogger logger, long id);

	[LoggerMessage(EventId = 9, Level = LogLevel.Debug, Message = "Requesting {Id} manga details")]
	public static partial void RequestingMangaDetails(this ILogger logger, long id);

	[LoggerMessage(EventId = 10, Level = LogLevel.Debug, Message = "Requesting {Id} anime seiyu")]
	public static partial void RequestingSeiyuDetails(this ILogger logger, long id);

	[LoggerMessage(EventId = 11, Level = LogLevel.Warning, Message = "Tenrai shared Retry-After cooldown engaged for {RetryAfter}")]
	public static partial void TenraiCooldownEngaged(this ILogger logger, TimeSpan retryAfter);
}
