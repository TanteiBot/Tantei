// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.UpdatesProviders.Base.Search;

internal abstract class MediaSearchServiceBase(SearchOrchestrator _orchestrator, SearchProviderIdentity _identity, int _minimumQueryLength)
	: IMediaSearchProvider
{
	public SearchProviderIdentity Identity => _identity;

	public int MinimumQueryLength => _minimumQueryLength;

	public abstract Task<SearchEvaluation> EvaluateAsync(SearchRequest request, CancellationToken cancellationToken);

	public abstract SearchFailure Classify(Exception exception);

	protected Task RunSearchAsync(
		ISearchInvocation invocation,
		string query,
		PickerMediaKind mediaKind,
		SearchTypeFilter? filter,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(invocation);
		ArgumentNullException.ThrowIfNull(query);
		var request = new SearchRequest(MatchKey.Create(query), query, mediaKind, filter);
		return _orchestrator.RunAsync(this, invocation, request, cancellationToken);
	}
}
