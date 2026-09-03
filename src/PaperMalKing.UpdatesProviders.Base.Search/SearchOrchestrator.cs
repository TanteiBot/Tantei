// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.UpdatesProviders.Base.Search;

internal sealed class SearchOrchestrator(
	SearchPicker _picker,
	TimeProvider _timeProvider,
	ILogger<SearchOrchestrator> _logger)
{
	public const int MinimumQueryTextElements = 3;

	public Task RunAsync(
		IMediaSearchProvider provider,
		ISearchInvocation invocation,
		SearchRequest request,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(provider);
		ArgumentNullException.ThrowIfNull(invocation);
		return this.RunCoreAsync(provider, invocation, request, cancellationToken);
	}

	private async Task RunCoreAsync(
		IMediaSearchProvider provider,
		ISearchInvocation invocation,
		SearchRequest request,
		CancellationToken cancellationToken)
	{
		request = request with { RequesterId = invocation.DiscordUserId, IncludeNsfw = invocation.IncludeNsfw };
		var searchId = Guid.NewGuid();
		var context = new PickerSearchContext(
			request.RawQuery,
			request.MediaKind,
			request.Filter,
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
			await target.EditOriginalAsync(PickerView.Terminal(SearchMessages.MissingPermissions), cancellationToken).ConfigureAwait(false);
			return;
		}

		if (new StringInfo(request.RawQuery).LengthInTextElements < MinimumQueryTextElements)
		{
			await target.EditOriginalAsync(PickerView.Terminal(SearchMessages.QueryTooShort), cancellationToken).ConfigureAwait(false);
			return;
		}

		if (request.QueryKey.IsEmpty)
		{
			await target.EditOriginalAsync(PickerView.Terminal(SearchMessages.QueryWithoutLettersOrDigits), cancellationToken).ConfigureAwait(false);
			return;
		}

		SearchEvaluation evaluation;
		try
		{
			evaluation = await provider.EvaluateAsync(request, cancellationToken).ConfigureAwait(false);
		}
#pragma warning disable CA1031
		catch (Exception exception)
#pragma warning restore CA1031
		{
			var failure = provider.Classify(exception);
			failure.Log(_logger);
			await target.EditOriginalAsync(PickerView.Terminal(failure.UserMessage), cancellationToken).ConfigureAwait(false);
			return;
		}

		_logger.SearchCompleted(evaluation.Kind, evaluation.FloorSurvivorCount, evaluation.Results.Count);
		switch (evaluation.Kind)
		{
			case SearchOutcomeKind.NoResults:
				await target.EditOriginalAsync(PickerView.Terminal(SearchMessages.NoResults(request.RawQuery)), cancellationToken).ConfigureAwait(false);
				break;
			case SearchOutcomeKind.TypeFilterEmpty:
				await target.EditOriginalAsync(PickerView.Terminal(SearchMessages.TypeFilterEmpty(request.RawQuery)), cancellationToken).ConfigureAwait(false);
				break;
			case SearchOutcomeKind.AutoPosted:
				await this.AutoPostAsync(target, evaluation.AutoPostResult!, context, cancellationToken).ConfigureAwait(false);
				break;
			default:
				await _picker.OpenAsync(searchId, evaluation.Results, context, target, provider.Identity.DisplayName).ConfigureAwait(false);
				break;
		}
	}

	private async Task AutoPostAsync(
		IPickerMessageTarget target,
		SearchResult result,
		PickerSearchContext context,
		CancellationToken cancellationToken)
	{
		try
		{
			await target.SendPublicAsync(result.BuildEmbed(context), cancellationToken).ConfigureAwait(false);
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

			await this.TryPushAsync(() => target.EditOriginalAsync(PickerView.Terminal(SearchMessages.PostFailed), cancellationToken)).ConfigureAwait(false);
			return;
		}

		await this.TryPushAsync(() => target.DeleteOriginalAsync(cancellationToken)).ConfigureAwait(false);
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
