// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using GraphQL.Client.Http;
using Microsoft.Extensions.Logging;
using PaperMalKing.AniList.Wrapper.Abstractions;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.Database.Models.AniList;
using PaperMalKing.UpdatesProviders.Base.Search;

namespace PaperMalKing.AniList.UpdateProvider.Search;

internal sealed class AniListMediaSearchService(
	IAniListClient _client,
	ILogger<AniListMediaSearchService> _logger,
	SearchOrchestrator _orchestrator) : MediaSearchServiceBase(_orchestrator, new("AniList", "anilist"), 1)
{
	public Task SearchAnimeAsync(ISearchInvocation invocation, string query, MediaFormat? format, CancellationToken cancellationToken)
		=> this.RunSearchAsync(invocation, query, PickerMediaKind.Anime, SearchTypeFilter.From(format), cancellationToken);

	public Task SearchMangaAsync(ISearchInvocation invocation, string query, MediaFormat? format, CancellationToken cancellationToken)
		=> this.RunSearchAsync(invocation, query, PickerMediaKind.Manga, SearchTypeFilter.From(format), cancellationToken);

	public override async Task<SearchEvaluation> EvaluateAsync(SearchRequest request, CancellationToken cancellationToken)
	{
		var features = AniListUserFeatures.SearchDefault;
		var options = (RequestOptions)features;

		var mediaType = request.MediaKind == PickerMediaKind.Manga ? ListType.Manga : ListType.Anime;
		var format = request.Filter?.As<MediaFormat>();
		var response = await _client.SearchMediaAsync(request.RawQuery, mediaType, options, format, userId: null, cancellationToken).ConfigureAwait(false);

		var titleLanguage = response.User?.Options.TitleLanguage ?? TitleLanguage.Default;
		var scoreFormat = response.User?.MediaListOptions?.ScoreFormat ?? ScoreFormat.POINT_100;

		var results = request.IncludeNsfw
			? response.Page.Values.AsEnumerable()
			: response.Page.Values.Where(static media => !media.IsAdult);

		var candidates = results.Select(media => AniListMediaCandidate.Create(
			media,
			titleLanguage,
			scoreFormat,
			(context, ct) => SearchEmbedBuilder.BuildAsync(_client, media, features, titleLanguage, context.RequesterDisplayName, context.RequesterAvatarUrl, _logger, ct)));

		return SearchEvaluator.Evaluate(request.QueryKey, candidates);
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
