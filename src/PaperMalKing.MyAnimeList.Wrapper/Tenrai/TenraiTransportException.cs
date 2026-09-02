// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

public sealed class TenraiTransportException(TenraiAttemptFacts facts, Exception innerException)
	: Exception("Tenrai enrichment request did not complete", innerException)
{
	public TenraiAttemptFacts Facts { get; } = facts;
}
