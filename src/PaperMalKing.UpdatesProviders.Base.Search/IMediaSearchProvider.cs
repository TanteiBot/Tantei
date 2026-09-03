// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.UpdatesProviders.Base.Search;

internal interface IMediaSearchProvider
{
	SearchProviderIdentity Identity { get; }

	Task<SearchEvaluation> EvaluateAsync(SearchRequest request, CancellationToken cancellationToken);

	SearchFailure Classify(Exception exception);
}
