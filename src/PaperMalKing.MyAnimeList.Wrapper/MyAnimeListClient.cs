// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using JikanDotNet;
using JikanDotNet.Exceptions;
using Microsoft.Extensions.Logging;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.Base;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Types;
using PaperMalKing.MyAnimeList.Wrapper.Parsers;
using AnimeListEntry = PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList.AnimeListEntry;
using MangaListEntry = PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList.MangaListEntry;

namespace PaperMalKing.MyAnimeList.Wrapper;

[SuppressMessage("Design", "CA1031:Do not catch general exception types")]
public sealed class MyAnimeListClient : IMyAnimeListClient
{
	private readonly HttpClient _unofficialApiHttpClient;
	private readonly HttpClient _officialApiHttpClient;
	private readonly IJikan _jikanClient;
	private readonly ILogger<MyAnimeListClient> _logger;

	public MyAnimeListClient(ILogger<MyAnimeListClient> logger, HttpClient unofficialApiHttpClient, HttpClient officialApiHttpClient, IJikan jikanClient)
	{
		this._logger = logger;
		this._unofficialApiHttpClient = unofficialApiHttpClient;
		this._officialApiHttpClient = officialApiHttpClient;
		this._jikanClient = jikanClient;
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

		return await browsingContext.OpenAsync(htmlResponse => htmlResponse.Content(stream), cancellationToken).ConfigureAwait(false);
	}

	public async Task<User> GetUserAsync(string username, ParserOptions options, CancellationToken cancellationToken = default)
	{
		if (options == ParserOptions.None)
		{
			throw new ArgumentException("No reason to parse profile without anime/manga lists and favorites",
				nameof(options)); // TODO Replace with domain exception
		}

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
		var url = Constants.BASE_OFFICIAL_API_URL + TListType.LatestUpdatesUrl(username, requestOptions);
		var response = (ListQueryResult<TE, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>)(await this._officialApiHttpClient
			.GetFromJsonAsync(url, typeof(ListQueryResult<TE, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>), JsonSGContext.Default,
				cancellationToken).ConfigureAwait(false) ?? ListQueryResult<TE, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>.Empty);

		return response?.Data ?? Array.Empty<TE>();
	}

	public async Task<MediaInfo> GetAnimeDetailsAsync(long id,  CancellationToken cancellationToken = default)
	{
		this._logger.LogDebug("Requesting {Id} anime details", id);
		try
		{
			var anime = await this._jikanClient.GetAnimeAsync(id, cancellationToken).ConfigureAwait(false);
			return new MediaInfo()
			{
				Demographic = anime.Data.Demographics.Select(static x => x.Name).ToArray(),
				Themes = anime.Data.Themes.Select(static x => x.Name).ToArray(),
			};
		}
		catch (JikanRequestException jre)
		{
			this._logger.LogWarning(jre, "Error happened in Jikan when requesting anime details for {Id}", id);
		}
		catch (JikanValidationException jve)
		{
			this._logger.LogWarning(jve, "Validation error happened in Jikan when requesting anime details for {Id}", id);
		}
		catch (Exception ex)
		{
			this._logger.LogWarning(ex, "Some non-predicted error happened in Jikan when requesting anime details for {Id}", id);
		}
		return MediaInfo.Empty;
	}

	public async Task<MediaInfo> GetMangaDetailsAsync(long id, CancellationToken cancellationToken = default)
	{
		this._logger.LogDebug("Requesting {Id} manga details", id);
		try
		{
			var manga = await this._jikanClient.GetMangaAsync(id, cancellationToken).ConfigureAwait(false);
			return new()
			{
				Demographic = manga.Data.Demographics.Select(static x => x.Name).ToArray(),
				Themes = manga.Data.Themes.Select(static x => x.Name).ToArray(),
			};
		}
		catch (JikanRequestException jre)
		{
			this._logger.LogWarning(jre, "Error happened in Jikan when requesting manga details for {Id}", id);
		}
		catch (JikanValidationException jve)
		{
			this._logger.LogWarning(jve, "Validation error happened in Jikan when requesting manga details for {Id}", id);
		}
		catch (Exception ex)
		{
			this._logger.LogWarning(ex, "Some non-predicted error happened in Jikan when requesting manga details for {Id}", id);
		}

		return MediaInfo.Empty;
	}

	public async Task<IReadOnlyList<SeyuInfo>> GetAnimeSeiyuAsync(long id, CancellationToken cancellationToken = default)
	{
		this._logger.LogDebug("Requesting {Id} anime seiyu", id);
		try
		{
			var animeCharacters = await this._jikanClient.GetAnimeCharactersAsync(id, cancellationToken).ConfigureAwait(false);
			return animeCharacters.Data.SelectMany(x => x.VoiceActors).Where(x => string.Equals(x.Language, "Japanese", StringComparison.Ordinal)).Select(x => new SeyuInfo()
			{
				Name = x.Person.Name,
				Url = x.Person.Url,
			}).ToArray();
		}
		catch (JikanRequestException jre)
		{
			this._logger.LogWarning(jre, "Error happened in Jikan when requesting anime seiyu for {Id}", id);
		}
		catch (JikanValidationException jve)
		{
			this._logger.LogWarning(jve, "Validation error happened in Jikan when requesting anime seiyu for {Id}", id);
		}
		catch (Exception ex)
		{
			this._logger.LogWarning(ex, "Some non-predicted error happened in Jikan when requesting anime seiyu for {Id}", id);
		}

		return Array.Empty<SeyuInfo>();
	}
}

internal sealed class ListQueryResult<T, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>
	where T : BaseListEntry<TNode, TStatus, TMediaType, TNodeStatus, TListStatus>
	where TNode : BaseListEntryNode<TMediaType, TNodeStatus>
	where TStatus : BaseListEntryStatus<TListStatus>
	where TMediaType : unmanaged, Enum
	where TNodeStatus : unmanaged, Enum
	where TListStatus : unmanaged, Enum
{
	[JsonPropertyName("data")]
	[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
	public required T[] Data { get; init; }

	public static ListQueryResult<T, TNode, TStatus, TMediaType, TNodeStatus, TListStatus> Empty { get; } = new()
	{
		Data = Array.Empty<T>(),
	};
}

[JsonSerializable(
	typeof(ListQueryResult<AnimeListEntry, AnimeListEntryNode, AnimeListEntryStatus, AnimeMediaType, AnimeAiringStatus, AnimeListStatus>))]
[JsonSerializable(
	typeof(ListQueryResult<MangaListEntry, MangaListEntryNode, MangaListEntryStatus, MangaMediaType, MangaPublishingStatus, MangaListStatus>))]
[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
internal sealed partial class JsonSGContext : JsonSerializerContext
{ }