// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.Base;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Types;
using PaperMalKing.MyAnimeList.Wrapper.Parsers;

namespace PaperMalKing.MyAnimeList.Wrapper;

public sealed class MyAnimeListClient : IMyAnimeListClient
{
	private readonly HttpClient _unofficialApiHttpClient;
	private readonly HttpClient _officialApiHttpClient;
	private readonly ILogger<MyAnimeListClient> _logger;
	public MyAnimeListClient(ILogger<MyAnimeListClient> logger, HttpClient unofficialApiHttpClient, HttpClient officialApiHttpClient)
	{
		this._logger = logger;
		this._unofficialApiHttpClient = unofficialApiHttpClient;
		this._officialApiHttpClient = officialApiHttpClient;
	}

	private async Task<HttpResponseMessage> GetAsync(string url, CancellationToken cancellationToken = default)
	{
		var response = await this._unofficialApiHttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
								 .ConfigureAwait(false);
		return response.EnsureSuccessStatusCode();
	}

	private async Task<IDocument> GetAsHtmlAsync(string url, CancellationToken cancellationToken = default)
	{
		using var response = await this.GetAsync(url, cancellationToken).ConfigureAwait(false);
		using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
		#pragma warning disable CA2000
		var browsingContext = new BrowsingContext();
		#pragma warning restore CA2000

		var document = await browsingContext.OpenAsync(response => response.Content(stream, false), cancellationToken).ConfigureAwait(false);
		return document;
	}

	public async Task<User> GetUserAsync(string username, ParserOptions options, CancellationToken cancellationToken = default)
	{
		if (options == ParserOptions.None)
			throw new ArgumentException("No reason to parse profile without anime/manga lists and favorites",
				nameof(options)); // TODO Replace with domain exception
		this._logger.LogDebug("Requesting {@Username} profile", username);
		username = WebUtility.UrlEncode(username);
		var requestUrl = Constants.PROFILE_URL + username;
		using var document = await this.GetAsHtmlAsync(requestUrl, cancellationToken).ConfigureAwait(false);
		this._logger.LogTrace("Starting parsing {@Username} profile", username);
		var user = UserProfileParser.Parse(document, options);
		this._logger.LogTrace("Ended parsing {@Username} profile", username);
		return user;
	}

	public async Task<string> GetUsernameAsync(uint id, CancellationToken cancellationToken = default)
	{
		var url = $"{Constants.COMMENTS_URL}{id}";
		this._logger.LogDebug("Requesting username by id {@Id}", id);
		using var document = await this.GetAsHtmlAsync(url, cancellationToken).ConfigureAwait(false);
		return CommentsParser.Parse(document);
	}

	public async Task<IReadOnlyList<TE>>
		GetLatestListUpdatesAsync<TE, TListType, TRequestOptions, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>(
			string username, TRequestOptions requestOptions, CancellationToken cancellationToken = default)
		where TE : BaseListEntry<TNode, TStatus, TMediaType, TNodeStatus, TListStatus>
		where TListType : IListType
		where TRequestOptions : unmanaged, Enum
		where TNode : BaseListEntryNode<TMediaType, TNodeStatus>
		where TStatus : BaseListEntryStatus<TListStatus>
		where TMediaType : unmanaged, Enum
		where TNodeStatus : unmanaged, Enum
		where TListStatus : unmanaged, Enum
	{
		this._logger.LogDebug("Requesting {@Username} {@Type} list", username, TListType.ListEntryType);

		username = WebUtility.UrlEncode(username);
		var url = $"{Constants.BASE_OFFICIAL_API_URL}{TListType.LatestUpdatesUrl(username, requestOptions)}";
		using var response = await this._officialApiHttpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

		var updates = await response.Content
									.ReadFromJsonAsync<ListQueryResult<TE, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>>(
										JsonSerializerOptions.Default, cancellationToken).ConfigureAwait(false);

		return updates?.Data ?? Array.Empty<TE>();
	}

}
sealed file class ListQueryResult<T, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>
	where T : BaseListEntry<TNode, TStatus, TMediaType, TNodeStatus, TListStatus>
	where TNode : BaseListEntryNode<TMediaType, TNodeStatus>
	where TStatus : BaseListEntryStatus<TListStatus>
	where TMediaType : unmanaged, Enum
	where TNodeStatus : unmanaged, Enum
	where TListStatus : unmanaged, Enum

{
	[JsonPropertyName("data")]
	public required T[] Data { get; init; }
}