// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using GraphQL.Client.Http;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common.Enums;
using PaperMalKing.Shikimori.Wrapper.Abstractions;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;
using PaperMalKing.Shikimori.Wrapper.Responses;

namespace PaperMalKing.Shikimori.Wrapper;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Will be handled by its parent")]
public sealed class ShikiClient(HttpClient _httpClient, ILogger<ShikiClient> _logger, GraphQLHttpClient _graphQlClient) : IShikiClient
{
	public async Task<UserInfo> GetUserByNicknameAsync(string nickname, CancellationToken cancellationToken)
	{
		_logger.RequestingUserInfo(nickname);

		var result = await _graphQlClient.SendQueryAsync<UserInfoResponse>(new(Queries.UserByNicknameQuery, new { nickname }), cancellationToken);

		return result.Data.Users[0];
	}

	public async Task<UserInfo> GetUserByIdAsync(uint userId, CancellationToken cancellationToken)
	{
		_logger.RequestingUserInfo(userId);

		var query = string.Format(CultureInfo.InvariantCulture, Queries.UserByIdQuery, userId);
		var result = await _graphQlClient.SendQueryAsync<UserInfoResponse>(new GraphQLHttpRequest(query), cancellationToken);

		return result.Data.Users[0];
	}

	public async Task<Favourites> GetUserFavouritesAsync(uint userId, CancellationToken cancellationToken)
	{
		_logger.RequestingFavorites(userId);
		var url = $"{Constants.BaseUsersApiUrl}/{userId}/favourites";
		var favs = await _httpClient.GetFromJsonAsync(url, JsonContext.Default.Favourites, cancellationToken);
		return favs!;
	}

	public async Task<Paginatable<History>> GetUserHistoryAsync(uint userId, uint page, byte limit, HistoryRequestOptions options, CancellationToken cancellationToken)
	{
		var url = $"{Constants.BaseUsersApiUrl}/{userId}/history";
		limit = limit > Constants.HistoryLimit ? Constants.HistoryLimit : limit;
		_logger.RequestingHistoryPage(userId, page);

		using var content = new MultipartFormDataContent
		{
			{ new StringContent(page.ToString(CultureInfo.InvariantCulture)), "page" },
			{ new StringContent(limit.ToString(CultureInfo.InvariantCulture)), "limit" },
		};
		if (options != HistoryRequestOptions.Any)
		{
			content.Add(new StringContent(options.ToInvariantString()), "target_type");
		}

		using var rm = new HttpRequestMessage(HttpMethod.Get, url)
		{
			Content = content,
		};
		using var response = await _httpClient.SendAsync(rm, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

		if (!response.IsSuccessStatusCode)
		{
			return Paginatable<History>.Empty;
		}

		var data = (await response.Content.ReadFromJsonAsync(JsonContext.Default.HistoryArray, cancellationToken))!;
		var hasNextPage = data.Length == limit + 1;
		return new(data, hasNextPage);
	}

	public async Task<TMedia?> GetMediaAsync<TMedia>(ulong id, ListEntryType type, RequestOptions options, CancellationToken cancellationToken)
		where TMedia : BaseMedia
	{
		var request = GetRequestForMedia(id, type, options);
		_logger.RequestingMedia(id, type, options);

		var response = await _graphQlClient.SendQueryAsync<MediaResponse<TMedia>>(request, cancellationToken);

		return response.Data.Media.FirstOrDefault();
	}

	private static GraphQLHttpRequest GetRequestForMedia(ulong id, ListEntryType type, RequestOptions requestOptions)
	{
		var query = type == ListEntryType.Anime ? Queries.GetAnimeQuery(requestOptions) : Queries.GetMangaQuery(requestOptions);

		return new(query, new { ids = id.ToString(CultureInfo.InvariantCulture) });
	}

	public async Task<IReadOnlyList<UserAchievement>> GetUserAchievementsAsync(uint userId, CancellationToken cancellationToken)
	{
		const string url = $"{Constants.BaseApiUrl}/achievements";
		_logger.RequestingUserAchievements(userId);
		using var rm = new HttpRequestMessage(HttpMethod.Get, url)
		{
			Content = new MultipartFormDataContent
			{
				{ new StringContent(userId.ToString("D", CultureInfo.InvariantCulture)), "user_id" },
			},
		};
		using var response = await _httpClient.SendAsync(rm, HttpCompletionOption.ResponseContentRead, cancellationToken);
		var achievements = (await response.Content.ReadFromJsonAsync(JsonContext.Default.UserAchievementArray, cancellationToken))!;
		var r = new List<UserAchievement>(achievements.Length);
		r.AddRange(achievements.Where(x => x is { Level: > 0 }).GroupBy(x => x.Id, StringComparer.Ordinal)
							   .Select(userAchievement => new UserAchievement(userAchievement.Key, userAchievement.Max(x => x.Level))));

		return r;
	}

	public async Task<byte[]?> GetImageContentAsync(string url, CancellationToken cancellationToken)
	{
		try
		{
			return await _httpClient.GetByteArrayAsync(url, cancellationToken);
		}
		catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
		{
			_logger.ImageNotFound(url);
		}

		return null;
	}
}