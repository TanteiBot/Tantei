// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

internal abstract record TenraiEnrichmentOutcome<TValue>
{
	private TenraiEnrichmentOutcome()
	{
	}

	public sealed record Enriched(TValue Value) : TenraiEnrichmentOutcome<TValue>;

	public sealed record NotFound : TenraiEnrichmentOutcome<TValue>;

	public sealed record Suppressed(TenraiSuppression Reason) : TenraiEnrichmentOutcome<TValue>;

	public sealed record Failed(TenraiFailureKind Kind, int? Status, TenraiAttemptFacts Facts, long ElapsedMilliseconds)
		: TenraiEnrichmentOutcome<TValue>;
}
