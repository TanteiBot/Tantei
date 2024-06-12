// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common.Enums;
using PaperMalKing.Shikimori.Wrapper.Abstractions;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

namespace PaperMalKing.Shikimori.Wrapper;

[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Will be handled by its parent")]
public sealed class ShikiClient(HttpClient _httpClient, ILogger<ShikiClient> _logger) : IShikiClient
{
	public async Task<UserInfo> GetUserAsync(string nickname, CancellationToken cancellationToken = default)
	{
		_logger.RequestingUserInfo(nickname);

		nickname = WebUtility.UrlEncode(nickname);
		var url = $"{Constants.BaseUsersApiUrl}/{nickname}";

		using var rm = new HttpRequestMessage(HttpMethod.Get, url)
		{
			Content = new MultipartFormDataContent
			{
				{ new StringContent("1"), "is_nickname" },
			},
		};

		using var response = await _httpClient.SendAsync(rm, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
		return (await response.Content.ReadFromJsonAsync(JsonContext.Default.UserInfo, cancellationToken))!;
	}

	public async Task<Favourites> GetUserFavouritesAsync(uint userId, CancellationToken cancellationToken = default)
	{
		_logger.RequestingFavorites(userId);
		var url = $"{Constants.BaseUsersApiUrl}/{userId}/favourites";
		var favs = await _httpClient.GetFromJsonAsync(url, JsonContext.Default.Favourites, cancellationToken);
		return favs!;
	}

	public async Task<Paginatable<History[]>> GetUserHistoryAsync(uint userId, uint page, byte limit, HistoryRequestOptions options, CancellationToken cancellationToken = default)
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

		var data = (await response.Content.ReadFromJsonAsync(JsonContext.Default.HistoryArray, cancellationToken))!;
		var hasNextPage = data.Length == limit + 1;
		return new(data, hasNextPage);
	}

	public Task<TMedia?> GetMediaAsync<TMedia>(ulong id, ListEntryType type, CancellationToken cancellationToken = default)
		where TMedia : BaseMedia
	{
		var url = BuildUrlForRequestingMedia(id, type);
		_logger.RequestingMedia(id, type);
		return _httpClient.GetFromJsonAsync<TMedia>(url, cancellationToken);
	}

	private static string BuildUrlForRequestingMedia(ulong id, ListEntryType type) =>
		$"{Constants.BaseApiUrl}/{(type == ListEntryType.Anime ? "animes" : "mangas")}/{id}";

	public async Task<IReadOnlyList<Role>> GetMediaStaffAsync(ulong id, ListEntryType type, CancellationToken cancellationToken = default)
	{
		var url = $"{BuildUrlForRequestingMedia(id, type)}/roles";
		_logger.RequestingStaff(id, type);
		var roles = await _httpClient.GetFromJsonAsync(url, JsonContext.Default.ListRole, cancellationToken);
		roles!.RemoveAll(x => x.Person is null);
		roles.TrimExcess();
		return roles;
	}

	public Task<UserInfo> GetUserInfoAsync(uint userId, CancellationToken cancellationToken = default)
	{
		var url = $"{Constants.BaseUsersApiUrl}/{userId}/info";
		_logger.RequestingUserInfo(userId);
		return _httpClient.GetFromJsonAsync(url, JsonContext.Default.UserInfo, cancellationToken)!;
	}

	public async Task<IReadOnlyList<UserAchievement>> GetUserAchievementsAsync(uint userId, CancellationToken cancellationToken = default)
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
}