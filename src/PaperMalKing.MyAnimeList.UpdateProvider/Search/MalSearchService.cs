// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;
using PaperMalKing.UpdatesProviders.Base.Search;
using Polly.RateLimiting;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal sealed class MalSearchService(IMyAnimeListClient _client, SearchOrchestrator _orchestrator) : MediaSearchServiceBase(_orchestrator)
{
	private const int MinimumQueryTextElements = 3;
	private static readonly SearchProviderIdentity ProviderIdentity = new("MyAnimeList", "mal");

	public override SearchProviderIdentity Identity => ProviderIdentity;

	public override int MinimumQueryLength => MinimumQueryTextElements;

	public Task SearchAnimeAsync(ISearchInvocation invocation, string query, AnimeMediaType? mediaType, CancellationToken cancellationToken)
		=> this.RunSearchAsync(invocation, query, PickerMediaKind.Anime, SearchTypeFilter.From(mediaType), cancellationToken);

	public Task SearchMangaAsync(ISearchInvocation invocation, string query, MangaMediaType? mediaType, CancellationToken cancellationToken)
		=> this.RunSearchAsync(invocation, query, PickerMediaKind.Manga, SearchTypeFilter.From(mediaType), cancellationToken);

	public override async Task<SearchEvaluation> EvaluateAsync(SearchRequest request, CancellationToken cancellationToken)
	{
		if (request.MediaKind == PickerMediaKind.Manga)
		{
			var mangaResults = await _client.SearchMangaAsync(request.RawQuery, request.IncludeNsfw, cancellationToken).ConfigureAwait(false);
			var mangaFilter = request.Filter?.As<MangaMediaType>();
			var mangaCandidates = mangaResults.Select(result => MalMediaCandidate.Create<MangaMediaType, MangaPublishingStatus>(result, mangaFilter));
			return SearchEvaluator.Evaluate(request.QueryKey, mangaCandidates, applyTypeFilter: mangaFilter.HasValue);
		}

		var animeResults = await _client.SearchAnimeAsync(request.RawQuery, request.IncludeNsfw, cancellationToken).ConfigureAwait(false);
		var animeFilter = request.Filter?.As<AnimeMediaType>();
		var animeCandidates = animeResults.Select(result => MalMediaCandidate.Create<AnimeMediaType, AnimeAiringStatus>(result, animeFilter));
		return SearchEvaluator.Evaluate(request.QueryKey, animeCandidates, applyTypeFilter: animeFilter.HasValue);
	}

	public override SearchFailure Classify(Exception exception)
	{
		if (exception is HttpRequestException { StatusCode: HttpStatusCode.Forbidden })
		{
			return new(SearchMessages.Busy(ProviderIdentity.DisplayName), static logger => logger.OfficialApiForbidden());
		}

		if (exception is RateLimiterRejectedException)
		{
			return new(SearchMessages.Busy(ProviderIdentity.DisplayName), static logger => logger.RateLimiterQueueRejected());
		}

		return new(SearchMessages.Failed(ProviderIdentity.DisplayName), logger => logger.SearchFailed(exception));
	}
}
