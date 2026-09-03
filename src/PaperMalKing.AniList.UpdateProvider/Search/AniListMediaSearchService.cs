// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Net;
using GraphQL.Client.Http;
using Microsoft.EntityFrameworkCore;
using PaperMalKing.AniList.Wrapper.Abstractions;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.Database;
using PaperMalKing.Database.Models.AniList;
using PaperMalKing.UpdatesProviders.Base.Search;

namespace PaperMalKing.AniList.UpdateProvider.Search;

internal sealed class AniListMediaSearchService(
	IAniListClient _client,
	IDbContextFactory<DatabaseContext> _dbContextFactory,
	SearchOrchestrator _orchestrator) : IMediaSearchProvider
{
	private static readonly SearchProviderIdentity ProviderIdentity = new("AniList", "anilist");

	public SearchProviderIdentity Identity => ProviderIdentity;

	public Task SearchAnimeAsync(ISearchInvocation invocation, string query, MediaFormat? format, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(invocation);
		ArgumentNullException.ThrowIfNull(query);
		return _orchestrator.RunAsync(this, invocation, BuildRequest(query, PickerMediaKind.Anime, format), cancellationToken);
	}

	public Task SearchMangaAsync(ISearchInvocation invocation, string query, MediaFormat? format, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(invocation);
		ArgumentNullException.ThrowIfNull(query);
		return _orchestrator.RunAsync(this, invocation, BuildRequest(query, PickerMediaKind.Manga, format), cancellationToken);
	}

	[SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "Sqlite does not support async")]
	public async Task<SearchEvaluation> EvaluateAsync(SearchRequest request, CancellationToken cancellationToken)
	{
		using var db = _dbContextFactory.CreateDbContext();
		var dbUser = db.AniListUsers.TagWith("Query user when searching for media").TagWithCallSite().FirstOrDefault(su => su.DiscordUserId == request.RequesterId);

		var features = dbUser?.Features ?? AniListUserFeatures.Default;
		features = features & ~AniListUserFeatures.Genres & ~AniListUserFeatures.Mangaka & ~AniListUserFeatures.Studio;
		var options = (RequestOptions)features;

		var mediaType = request.MediaKind == PickerMediaKind.Manga ? ListType.Manga : ListType.Anime;
		var format = request.Filter?.As<MediaFormat>();
		var response = await _client.SearchMediaAsync(request.RawQuery, mediaType, options, format, dbUser?.Id, cancellationToken).ConfigureAwait(false);

		var titleLanguage = response.User?.Options.TitleLanguage ?? TitleLanguage.Default;
		var scoreFormat = response.User?.MediaListOptions?.ScoreFormat ?? ScoreFormat.POINT_100;

		var results = request.IncludeNsfw
			? response.Page.Values.AsEnumerable()
			: response.Page.Values.Where(static media => !media.IsAdult);

		var candidates = results.Select(media => AniListMediaCandidate.Create(
			media,
			titleLanguage,
			scoreFormat,
			context => SearchEmbedBuilder.Build(media, features, titleLanguage, context.RequesterDisplayName, context.RequesterAvatarUrl)));

		return SearchEvaluator.Evaluate(request.QueryKey, candidates);
	}

	public SearchFailure Classify(Exception exception)
	{
		if (exception is GraphQLHttpRequestException { StatusCode: HttpStatusCode.TooManyRequests })
		{
			return new(SearchMessages.Busy(ProviderIdentity.DisplayName), static logger => logger.RateLimiterQueueRejected());
		}

		return new(SearchMessages.Failed(ProviderIdentity.DisplayName), logger => logger.SearchFailed(exception));
	}

	private static SearchRequest BuildRequest(string query, PickerMediaKind mediaKind, MediaFormat? format) => new(
		MatchKey.Create(query),
		query,
		mediaKind,
		SearchTypeFilter.From(format));
}
