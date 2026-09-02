// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

internal static class TenraiEnrichmentReport
{
	public static void Report<TValue>(ILogger logger, string operation, long mediaId, TenraiEnrichmentOutcome<TValue> outcome)
	{
		switch (outcome)
		{
			case TenraiEnrichmentOutcome<TValue>.Enriched:
				break;
			case TenraiEnrichmentOutcome<TValue>.NotFound:
				logger.TenraiEnrichmentNotFound(operation, mediaId);
				break;
			case TenraiEnrichmentOutcome<TValue>.Suppressed { Reason: TenraiSuppression.CircuitOpen }:
				logger.TenraiEnrichmentCircuitSkipped(operation, mediaId);
				break;
			case TenraiEnrichmentOutcome<TValue>.Suppressed { Reason: TenraiSuppression.Cooldown }:
				logger.TenraiEnrichmentCooldownSuppressed(operation, mediaId);
				break;
			case TenraiEnrichmentOutcome<TValue>.Suppressed { Reason: TenraiSuppression.Queue }:
				logger.TenraiEnrichmentQueueRejected(operation, mediaId);
				break;
			case TenraiEnrichmentOutcome<TValue>.Failed failed:
				logger.TenraiEnrichmentFailed(operation, mediaId, failed.Kind, failed.Status, failed.Facts.RetryCount,
					failed.ElapsedMilliseconds, failed.Facts.RetryAfter);
				break;
			default:
				throw new UnreachableException();
		}
	}
}
