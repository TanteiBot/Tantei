// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using CommunityToolkit.Diagnostics;
using JikanDotNet;
using Microsoft.Extensions.Logging;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.Base;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Types;
using PaperMalKing.MyAnimeList.Wrapper.Parsers;

namespace PaperMalKing.MyAnimeList.Wrapper;

[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "We want to ignore exceptions")]
public sealed class MyAnimeListClient(ILogger<MyAnimeListClient> _logger, HttpClient _unofficialApiHttpClient, HttpClient _officialApiHttpClient, IJikan _jikanClient)
	: IMyAnimeListClient
{
	private async Task<HttpResponseMessage> GetAsync(string url, CancellationToken cancellationToken = default)
	{
		var response = await _unofficialApiHttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
		return response.EnsureSuccessStatusCode();
	}

	private async Task<IDocument> GetAsHtmlAsync(string url, CancellationToken cancellationToken = default)
	{
		using var response = await this.GetAsync(url, cancellationToken);
		await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
		#pragma warning disable CA2000
		var browsingContext = new BrowsingContext();
		#pragma warning restore CA2000
		return await browsingContext.OpenAsync(htmlResponse => htmlResponse.Content(stream), cancellationToken);
	}

	public async Task<User> GetUserAsync(string username, ParserOptions options, CancellationToken cancellationToken = default)
	{
		if (options == ParserOptions.None)
		{
			ThrowHelper.ThrowArgumentException("No reason to parse profile without anime/manga lists and favorites",
				nameof(options)); // TODO Replace with domain exception
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

	public async Task<string> GetUsernameAsync(uint id, CancellationToken cancellationToken = default)
	{
		var url = $"{Constants.CommentsUrl}{id}";
		_logger.RequestingUsername(id);
		using var document = await this.GetAsHtmlAsync(url, cancellationToken);
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

	public async Task<MediaInfo> GetAnimeDetailsAsync(long id, CancellationToken cancellationToken = default)
	{
		_logger.RequestingAnimeDetails(id);
		try
		{
			var anime = await _jikanClient.GetAnimeAsync(id, cancellationToken);
			return new()
			{
				Demographic = anime.Data.Demographics.Select(static x => x.Name).ToArray(),
				Themes = anime.Data.Themes.Select(static x => x.Name).ToArray(),
			};
		}
		catch (Exception ex)
		{
			_logger.ErrorHappenedInJikanWhenRequestingAnime(ex, id);
		}

		return MediaInfo.Empty;
	}

	public async Task<MediaInfo> GetMangaDetailsAsync(long id, CancellationToken cancellationToken = default)
	{
		_logger.RequestingMangaDetails(id);
		try
		{
			var manga = await _jikanClient.GetMangaAsync(id, cancellationToken);
			return new()
			{
				Demographic = manga.Data.Demographics.Select(static x => x.Name).ToArray(),
				Themes = manga.Data.Themes.Select(static x => x.Name).ToArray(),
			};
		}
		catch (Exception ex)
		{
			_logger.ErrorHappenedInJikanWhenRequestingManga(ex, id);
		}

		return MediaInfo.Empty;
	}

	public async Task<IReadOnlyList<SeyuInfo>> GetAnimeSeiyuAsync(long id, CancellationToken cancellationToken = default)
	{
		_logger.RequestingSeyuDetails(id);
		try
		{
			var animeCharacters = await _jikanClient.GetAnimeCharactersAsync(id, cancellationToken);
			return animeCharacters.Data.SelectMany(x => x.VoiceActors).Where(x => x.Language.Equals("Japanese", StringComparison.Ordinal))
								  .Select(x => new SeyuInfo
								  {
									  Name = x.Person.Name,
									  Url = x.Person.Url,
								  }).ToArray();
		}
		catch (Exception ex)
		{
			_logger.ErrorHappenedInJikanWhenRequestingSeyu(ex, id);
		}

		return [];
	}
}