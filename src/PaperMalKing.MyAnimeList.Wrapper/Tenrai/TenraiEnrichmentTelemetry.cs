// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

internal sealed class TenraiEnrichmentTelemetry
{
	private readonly AsyncLocal<TenraiEnrichmentAttempt?> _current = new();

	public TenraiEnrichmentAttempt? Current => this._current.Value;

	public TenraiEnrichmentAttempt Begin()
	{
		var attempt = new TenraiEnrichmentAttempt();
		this._current.Value = attempt;
		return attempt;
	}

	public void End() => this._current.Value = null;
}
