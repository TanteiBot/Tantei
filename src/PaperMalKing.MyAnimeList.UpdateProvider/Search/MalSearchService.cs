// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using System.Net;
using Microsoft.Extensions.Logging;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;
using Polly.RateLimiting;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal sealed class MalSearchService(
	IMyAnimeListClient _client,
	MalSearchPicker _picker,
	TimeProvider _timeProvider,
	ILogger<MalSearchService> _logger)
{
	public const int MinimumQueryTextElements = 3;

	public Task SearchAnimeAsync(ISearchInvocation invocation, string query, AnimeMediaType? mediaType, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(invocation);
		ArgumentNullException.ThrowIfNull(query);
		return this.SearchAsync(
			invocation,
			query,
			PickerMediaKind.Anime,
			mediaType?.ToString(),
			async (queryKey, includeNsfw, token) =>
			{
				var response = await _client.SearchAnimeAsync(query, includeNsfw, token).ConfigureAwait(false);
				return SearchEvaluation.From(SearchPipeline.Evaluate(queryKey, response, mediaType), PickerSearchResult.ForAnime);
			},
			cancellationToken);
	}

	public Task SearchMangaAsync(ISearchInvocation invocation, string query, MangaMediaType? mediaType, CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(invocation);
		ArgumentNullException.ThrowIfNull(query);
		return this.SearchAsync(
			invocation,
			query,
			PickerMediaKind.Manga,
			mediaType?.ToString(),
			async (queryKey, includeNsfw, token) =>
			{
				var response = await _client.SearchMangaAsync(query, includeNsfw, token).ConfigureAwait(false);
				return SearchEvaluation.From(SearchPipeline.Evaluate(queryKey, response, mediaType), PickerSearchResult.ForManga);
			},
			cancellationToken);
	}

	private async Task SearchAsync(
		ISearchInvocation invocation,
		string query,
		PickerMediaKind mediaKind,
		string? mediaTypeFilter,
		Func<MatchKey, bool, CancellationToken, Task<SearchEvaluation>> search,
		CancellationToken cancellationToken)
	{
		var searchId = Guid.NewGuid().ToString("N");
		var context = new PickerSearchContext(
			query,
			mediaKind,
			mediaTypeFilter,
			invocation.DiscordUserId,
			invocation.RequesterDisplayName,
			invocation.RequesterAvatarUrl,
			invocation.GuildId,
			invocation.ChannelId,
			_timeProvider.GetUtcNow());
		var target = invocation.Target;
		using var scope = _logger.SearchScope(searchId, context);
		if (!invocation.CanPostEmbed)
		{
			_logger.PermissionDenied();
			await target.EditOriginalAsync(PickerView.Terminal(SearchMessages.MissingPermissions)).ConfigureAwait(false);
			return;
		}

		if (new StringInfo(query).LengthInTextElements < MinimumQueryTextElements)
		{
			await target.EditOriginalAsync(PickerView.Terminal(SearchMessages.QueryTooShort)).ConfigureAwait(false);
			return;
		}

		var queryKey = MatchKey.Create(query);
		if (queryKey.IsEmpty)
		{
			await target.EditOriginalAsync(PickerView.Terminal(SearchMessages.QueryWithoutLettersOrDigits)).ConfigureAwait(false);
			return;
		}

		SearchEvaluation evaluation;
		try
		{
			evaluation = await search(queryKey, invocation.IncludeNsfw, cancellationToken).ConfigureAwait(false);
		}
#pragma warning disable CA1031
		catch (Exception exception)
#pragma warning restore CA1031
		{
			await this.ReportSearchFailureAsync(target, exception).ConfigureAwait(false);
			return;
		}

		_logger.SearchCompleted(evaluation.Kind, evaluation.FloorSurvivorCount, evaluation.Results.Count);
		switch (evaluation.Kind)
		{
			case SearchOutcomeKind.NoResults:
				await target.EditOriginalAsync(PickerView.Terminal(SearchMessages.NoResults(query))).ConfigureAwait(false);
				break;
			case SearchOutcomeKind.TypeFilterEmpty:
				await target.EditOriginalAsync(PickerView.Terminal(SearchMessages.TypeFilterEmpty(query))).ConfigureAwait(false);
				break;
			case SearchOutcomeKind.AutoPosted:
				await this.AutoPostAsync(target, evaluation.AutoPostResult!, context).ConfigureAwait(false);
				break;
			default:
				var opened = _picker.Open(searchId, evaluation.Results, context, target);
				await target.EditOriginalAsync(opened.View).ConfigureAwait(false);
				break;
		}
	}

	private async Task AutoPostAsync(IPickerMessageTarget target, PickerSearchResult result, PickerSearchContext context)
	{
		try
		{
			await target.SendPublicAsync(result.BuildEmbed(context)).ConfigureAwait(false);
		}
#pragma warning disable CA1031
		catch (Exception exception)
#pragma warning restore CA1031
		{
			if (SearchPostFailure.IsForbidden(exception))
			{
				_logger.PublicPostForbidden();
			}
			else
			{
				_logger.PublicPostFailed(exception);
			}

			await this.TryPushAsync(() => target.EditOriginalAsync(PickerView.Terminal(SearchMessages.PostFailed))).ConfigureAwait(false);
			return;
		}

		await this.TryPushAsync(target.DeleteOriginalAsync).ConfigureAwait(false);
	}

	private Task ReportSearchFailureAsync(IPickerMessageTarget target, Exception exception)
	{
		string message;
		if (exception is HttpRequestException { StatusCode: HttpStatusCode.Forbidden })
		{
			_logger.OfficialApiForbidden();
			message = SearchMessages.Busy;
		}
		else if (exception is RateLimiterRejectedException)
		{
			_logger.RateLimiterQueueRejected();
			message = SearchMessages.Busy;
		}
		else
		{
			_logger.SearchFailed(exception);
			message = SearchMessages.Failed;
		}

		return target.EditOriginalAsync(PickerView.Terminal(message));
	}

	private async Task TryPushAsync(Func<Task> push)
	{
		try
		{
			await push().ConfigureAwait(false);
		}
#pragma warning disable CA1031
		catch (Exception exception)
#pragma warning restore CA1031
		{
			_logger.TerminalStatePushFailed(exception);
		}
	}
}
