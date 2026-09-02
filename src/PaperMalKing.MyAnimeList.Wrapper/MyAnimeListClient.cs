// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization.Metadata;
using AngleSharp;
using AngleSharp.Dom;
using JikanDotNet;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common.Exceptions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.Base;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Types;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;
using PaperMalKing.MyAnimeList.Wrapper.Parsers;
using PaperMalKing.MyAnimeList.Wrapper.Tenrai;

namespace PaperMalKing.MyAnimeList.Wrapper;

[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We want to ignore exceptions")]
public sealed class MyAnimeListClient(
	ILogger<MyAnimeListClient> _logger,
	HttpClient _unofficialApiHttpClient,
	HttpClient _officialApiHttpClient,
	HttpClient _tenraiApiHttpClient,
	IJikan _jikanClient) : IMyAnimeListClient
{
	private const string AnimeSearchFields =
		"id,title,main_picture,alternative_titles,media_type,status,num_episodes,mean,start_date,start_season,num_list_users,genres{name},synopsis,nsfw";

	private const string MangaSearchFields =
		"id,title,main_picture,alternative_titles,media_type,status,num_chapters,num_volumes,mean,start_date,num_list_users,genres{name},synopsis,nsfw";

	private readonly TenraiClient _tenraiClient = new(_tenraiApiHttpClient);

	private static string CreateSearchUrl(string mediaPath, string query, string fields, bool includeNsfw) =>
		$"{Constants.BaseOfficialApiUrl}/{mediaPath}?q={Uri.EscapeDataString(query)}&limit=100&offset=0&fields={fields}{(includeNsfw ? "&nsfw=true" : string.Empty)}";

	private static string[] ValidNames(IEnumerable<CatalogReference>? entries) => entries?
		.Where(static entry => !string.IsNullOrWhiteSpace(entry.Name))
		.Select(static entry => entry.Name!)
		.ToArray() ?? [];

	private async Task<HttpResponseMessage> GetAsync(string url, CancellationToken cancellationToken)
	{
		var response = await _unofficialApiHttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
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

		_logger.RequestingProfile(username);
		username = WebUtility.UrlEncode(username);
		var requestUrl = Constants.ProfileUrl + username;
		using var document = await this.GetAsHtmlAsync(requestUrl, cancellationToken);
		_logger.StartingParsingProfile(username);
		var user = UserProfileParser.Parse(document, options);
		_logger.EndingParsingProfile(username);
		return user;
	}

	public async Task<string> GetUsernameAsync(uint id, CancellationToken cancellationToken)
	{
		var url = $"{Constants.CommentsUrl}{id}";
		_logger.RequestingUsername(id);
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
		_logger.RequestingList(username, TListType.ListEntryType);

		username = WebUtility.UrlEncode(username);
		var url = Constants.BaseOfficialApiUrl + TListType.LatestUpdatesUrl(username, requestOptions);
		var response = (ListQueryResult<TE, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>)(await _officialApiHttpClient
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
		var response = await _officialApiHttpClient.GetFromJsonAsync(url, jsonTypeInfo, cancellationToken);
		return response?.Results.Select(static envelope => envelope.Result).ToArray() ?? [];
	}

	public Task<MediaInfo> GetAnimeDetailsAsync(long id, CancellationToken cancellationToken)
	{
		_logger.RequestingAnimeDetails(id);
		return this.GetDetailsAsync("anime", id, this._tenraiClient.GetAnimeByIdAsync, cancellationToken);
	}

	public Task<MediaInfo> GetMangaDetailsAsync(long id, CancellationToken cancellationToken)
	{
		_logger.RequestingMangaDetails(id);
		return this.GetDetailsAsync("manga", id, this._tenraiClient.GetMangaByIdAsync, cancellationToken);
	}

	private async Task<MediaInfo> GetDetailsAsync(
		string operation,
		long id,
		Func<int, CancellationToken, Task<TenraiResponse<MediaResponse>>> request,
		CancellationToken cancellationToken)
	{
		try
		{
			var response = await request(checked((int)id), cancellationToken);
			var themes = ValidNames(response.Result.Data.Themes);
			var demographic = ValidNames(response.Result.Data.Demographics);
			return themes.Length is 0 && demographic.Length is 0
				? MediaInfo.Empty
				: new() { Themes = themes, Demographic = demographic, };
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			throw;
		}
		catch (Exception ex)
		{
			_logger.ErrorHappenedInTenraiWhenRequestingDetails(ex, operation, id);
			return MediaInfo.Empty;
		}
	}

	public async Task<IReadOnlyList<SeyuInfo>> GetAnimeSeiyuAsync(long id, CancellationToken cancellationToken)
	{
		_logger.RequestingSeyuDetails(id);
		try
		{
			var animeCharacters = await _jikanClient.GetAnimeCharactersAsync(id, cancellationToken);
			return [.. animeCharacters.Data.SelectMany(x => x.VoiceActors).Where(x => x.Language.Equals("Japanese", StringComparison.Ordinal))
								  .Select(x => new SeyuInfo
								  {
									  Name = x.Person.Name,
									  Url = x.Person.Url,
								  }),];
		}
		catch (Exception ex)
		{
			_logger.ErrorHappenedInJikanWhenRequestingSeyu(ex, id);
		}

		return [];
	}
}