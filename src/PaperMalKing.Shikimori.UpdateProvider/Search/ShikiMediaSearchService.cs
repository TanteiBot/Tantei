// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using GraphQL.Client.Http;
using PaperMalKing.Common.Enums;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.Wrapper.Abstractions;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.UpdatesProviders.Base.Search;

namespace PaperMalKing.Shikimori.UpdateProvider.Search;

internal sealed class ShikiMediaSearchService(
	IShikiClient _client,
	SearchOrchestrator _orchestrator) : MediaSearchServiceBase(_orchestrator, new("Shikimori", "shikimori"), 1)
{
	public Task SearchAnimeAsync(ISearchInvocation invocation, string query, AnimeKind? kind, CancellationToken cancellationToken)
		=> this.RunSearchAsync(invocation, query, PickerMediaKind.Anime, SearchTypeFilter.From(kind), cancellationToken);

	public Task SearchMangaAsync(ISearchInvocation invocation, string query, MangaKind? kind, CancellationToken cancellationToken)
		=> this.RunSearchAsync(invocation, query, PickerMediaKind.Manga, SearchTypeFilter.From(kind), cancellationToken);

	public override async Task<SearchEvaluation> EvaluateAsync(SearchRequest request, CancellationToken cancellationToken)
	{
		var features = ShikiUserFeatures.SearchDefault;
		const bool useRussian = false;

		if (request.MediaKind == PickerMediaKind.Manga)
		{
			var kind = request.Filter?.As<MangaKind>();
			var results = await _client.SearchMangaAsync(request.RawQuery, kind, request.IncludeNsfw, cancellationToken).ConfigureAwait(false);
			var candidates = results.Select(media => ShikiMediaCandidate.Create(media, ListEntryType.Manga, features, useRussian, kind?.ToGraphQlKind()));
			return SearchEvaluator.Evaluate(request.QueryKey, candidates, applyTypeFilter: kind.HasValue);
		}

		var animeKind = request.Filter?.As<AnimeKind>();
		var animeResults = await _client.SearchAnimeAsync(request.RawQuery, animeKind, request.IncludeNsfw, cancellationToken).ConfigureAwait(false);
		var animeCandidates = (request.IncludeNsfw ? animeResults : animeResults.Where(static media => !media.IsAdult))
			.Select(media => ShikiMediaCandidate.Create(media, ListEntryType.Anime, features, useRussian, animeKind?.ToGraphQlKind()));
		return SearchEvaluator.Evaluate(request.QueryKey, animeCandidates, applyTypeFilter: animeKind.HasValue);
	}

	public override SearchFailure Classify(Exception exception)
	{
		if (exception is GraphQLHttpRequestException { StatusCode: HttpStatusCode.TooManyRequests })
		{
			return new(SearchMessages.Busy(this.Identity.DisplayName), static logger => logger.RateLimiterQueueRejected());
		}

		return new(SearchMessages.Failed(this.Identity.DisplayName), logger => logger.SearchFailed(exception));
	}
}
