// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common.Exceptions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.Base;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Types;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;
using PaperMalKing.MyAnimeList.Wrapper.Parsers;

namespace PaperMalKing.MyAnimeList.Wrapper;

[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We want to ignore exceptions")]
public sealed class MyAnimeListClient : IMyAnimeListClient
{
	private const string AnimeSearchFields =
		"id,title,main_picture,alternative_titles,media_type,status,num_episodes,mean,start_date,start_season,num_list_users,genres{name},synopsis,nsfw";

	private const string MangaSearchFields =
		"id,title,main_picture,alternative_titles,media_type,status,num_chapters,num_volumes,mean,start_date,num_list_users,genres{name},synopsis,nsfw";

	private readonly IMyAnimeListEnrichment _enrichment;
	private readonly ILogger<MyAnimeListClient> _logger;
	private readonly HttpClient _officialApiHttpClient;
	private readonly HttpClient _unofficialApiHttpClient;

	internal MyAnimeListClient(
		ILogger<MyAnimeListClient> logger,
		HttpClient unofficialApiHttpClient,
		HttpClient officialApiHttpClient,
		IMyAnimeListEnrichment enrichment)
	{
		this._logger = logger;
		this._unofficialApiHttpClient = unofficialApiHttpClient;
		this._officialApiHttpClient = officialApiHttpClient;
		this._enrichment = enrichment;
	}

	private static string CreateSearchUrl(string mediaPath, string query, string fields, bool includeNsfw) =>
		$"{Constants.BaseOfficialApiUrl}/{mediaPath}?q={Uri.EscapeDataString(query)}&limit=100&offset=0&fields={fields}{(includeNsfw ? "&nsfw=true" : string.Empty)}";

	private async Task<HttpResponseMessage> GetAsync(string url, CancellationToken cancellationToken)
	{
		var response = await this._unofficialApiHttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
		return response.EnsureSuccessStatusCode();
	}

	private async Task<IDocument> GetAsHtmlAsync(string url, CancellationToken cancellationToken)
	{
		using var response = await this.GetAsync(url, cancellationToken);
		await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
#pragma warning disable CA2000
		var browsingContext = new BrowsingContext();
#pragma warning restore
		return await browsingContext.OpenAsync(htmlResponse => htmlResponse.Content(stream), cancellationToken);
	}

	public async Task<User> GetUserAsync(string username, ParserOptions options, CancellationToken cancellationToken)
	{
		if (options == ParserOptions.None)
		{
			ArgumentException.Throw<ParserOptions>("No reason to parse profile without anime/manga lists and favorites", nameof(options));
		}

		this._logger.RequestingProfile(username);
		username = WebUtility.UrlEncode(username);
		var requestUrl = Constants.ProfileUrl + username;
		using var document = await this.GetAsHtmlAsync(requestUrl, cancellationToken);
		this._logger.StartingParsingProfile(username);
		var user = UserProfileParser.Parse(document, options);
		this._logger.EndingParsingProfile(username);
		return user;
	}

	public async Task<string> GetUsernameAsync(uint id, CancellationToken cancellationToken)
	{
		var url = $"{Constants.CommentsUrl}{id}";
		this._logger.RequestingUsername(id);
		using var document = await this.GetAsHtmlAsync(url, cancellationToken);
		return CommentsParser.Parse(document);
	}

	public async Task<IReadOnlyList<TE>>
		GetLatestListUpdatesAsync<TE, TListType, TRequestOptions, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>(
			string username, TRequestOptions requestOptions, CancellationToken cancellationToken)
		where TE : BaseListEntry<TNode, TStatus, TMediaType, TNodeStatus, TListStatus>
		where TListType : IListType
		where TRequestOptions : unmanaged, Enum
		where TNode : BaseListEntryNode<TMediaType, TNodeStatus>
		where TStatus : BaseListEntryStatus<TListStatus>
		where TMediaType : unmanaged, Enum
		where TNodeStatus : unmanaged, Enum
		where TListStatus : unmanaged, Enum
	{
		this._logger.RequestingList(username, TListType.ListEntryType);

		username = WebUtility.UrlEncode(username);
		var url = Constants.BaseOfficialApiUrl + TListType.LatestUpdatesUrl(username, requestOptions);
		var response = (ListQueryResult<TE, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>)(await this._officialApiHttpClient
			.GetFromJsonAsync(url,
				typeof(ListQueryResult<TE, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>),
				JsonContext.Default,
				cancellationToken) ?? ListQueryResult<TE, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>.Empty);

		return response.Data;
	}

	public Task<IReadOnlyList<AnimeSearchResult>> SearchAnimeAsync(string query, bool includeNsfw, CancellationToken cancellationToken) =>
		this.SearchAsync("anime", query, AnimeSearchFields, includeNsfw, JsonContext.Default.SearchResponseAnimeSearchResult, cancellationToken);

	public Task<IReadOnlyList<MangaSearchResult>> SearchMangaAsync(string query, bool includeNsfw, CancellationToken cancellationToken) =>
		this.SearchAsync("manga", query, MangaSearchFields, includeNsfw, JsonContext.Default.SearchResponseMangaSearchResult, cancellationToken);

	private async Task<IReadOnlyList<TResult>> SearchAsync<TResult>(
		string mediaPath,
		string query,
		string fields,
		bool includeNsfw,
		JsonTypeInfo<SearchResponse<TResult>> jsonTypeInfo,
		CancellationToken cancellationToken)
	{
		var url = CreateSearchUrl(mediaPath, query, fields, includeNsfw);
		var response = await this._officialApiHttpClient.GetFromJsonAsync(url, jsonTypeInfo, cancellationToken);
		return response?.Results.Select(static envelope => envelope.Result).ToArray() ?? [];
	}

	public Task<MediaInfo> GetAnimeDetailsAsync(long id, CancellationToken cancellationToken) =>
		this._enrichment.GetAnimeDetailsAsync(id, cancellationToken);

	public Task<MediaInfo> GetMangaDetailsAsync(long id, CancellationToken cancellationToken) =>
		this._enrichment.GetMangaDetailsAsync(id, cancellationToken);

	public Task<IReadOnlyList<SeyuInfo>> GetAnimeSeiyuAsync(long id, CancellationToken cancellationToken) =>
		this._enrichment.GetAnimeSeiyuAsync(id, cancellationToken);
}
