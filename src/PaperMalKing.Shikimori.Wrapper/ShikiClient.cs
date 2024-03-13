// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Generic;
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

public sealed class ShikiClient : IShikiClient
{
	private readonly HttpClient _httpClient;
	private readonly ILogger<ShikiClient> _logger;

	public ShikiClient(HttpClient httpClient, ILogger<ShikiClient> logger)
	{
		this._httpClient = httpClient;
		this._logger = logger;
	}

	public async Task<UserInfo> GetUserAsync(string nickname, CancellationToken cancellationToken = default)
	{
		this._logger.RequestingUserInfo(nickname);

		nickname = WebUtility.UrlEncode(nickname);
		var url = $"{Constants.BaseUsersApiUrl}/{nickname}";

		using var rm = new HttpRequestMessage(HttpMethod.Get, url)
		{
			Content = new MultipartFormDataContent
			{
				#pragma warning disable CA2000
				// Call System.IDisposable.Dispose on object created by 'new StringContent("1")' before all references to it are out of scope
				{ new StringContent("1"), "is_nickname" },
				#pragma warning restore CA2000
			},
		};

		using var response = await this._httpClient.SendAsync(rm, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
		return (await response.Content.ReadFromJsonAsync(JsonSGContext.Default.UserInfo, cancellationToken))!;
	}

	public async Task<Favourites> GetUserFavouritesAsync(uint userId, CancellationToken cancellationToken = default)
	{
		this._logger.RequestingFavorites(userId);
		var url = $"{Constants.BaseUsersApiUrl}/{userId}/favourites";
		var favs = await this._httpClient.GetFromJsonAsync(url, JsonSGContext.Default.Favourites, cancellationToken);
		return favs!;
	}

	public async Task<Paginatable<History[]>> GetUserHistoryAsync(uint userId, uint page, byte limit, HistoryRequestOptions options, CancellationToken cancellationToken = default)
	{
		var url = $"{Constants.BaseUsersApiUrl}/{userId}/history";
		limit = limit > Constants.HistoryLimit ? Constants.HistoryLimit : limit;
		this._logger.RequestingHistoryPage(userId, page);

		#pragma warning disable CA2000
		// Call System.IDisposable.Dispose on object created by 'new StringContent("1")' before all references to it are out of scope
		using var content = new MultipartFormDataContent
		{
			{ new StringContent(page.ToString(CultureInfo.InvariantCulture)), "page" },
			{ new StringContent(limit.ToString(CultureInfo.InvariantCulture)), "limit" },
		};
		if (options != HistoryRequestOptions.Any)
		{
			content.Add(new StringContent(options.ToInvariantString()), "target_type");
		}
		#pragma warning restore CA2000

		using var rm = new HttpRequestMessage(HttpMethod.Get, url)
		{
			Content = content,
		};
		using var response = await this._httpClient.SendAsync(rm, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

		var data = (await response.Content.ReadFromJsonAsync(JsonSGContext.Default.HistoryArray, cancellationToken))!;
		var hasNextPage = data.Length == limit + 1;
		return new(data, hasNextPage);
	}

	public Task<TMedia?> GetMediaAsync<TMedia>(ulong id, ListEntryType type, CancellationToken cancellationToken = default)
		where TMedia : BaseMedia
	{
		var url = BuildUrlForRequestingMedia(id, type);
		this._logger.RequestingMedia(id, type);
		return this._httpClient.GetFromJsonAsync<TMedia>(url, cancellationToken);
	}

	private static string BuildUrlForRequestingMedia(ulong id, ListEntryType type) =>
		$"{Constants.BaseApiUrl}/{(type == ListEntryType.Anime ? "animes" : "mangas")}/{id}";

	public async Task<IReadOnlyList<Role>> GetMediaStaffAsync(ulong id, ListEntryType type, CancellationToken cancellationToken = default)
	{
		var url = $"{BuildUrlForRequestingMedia(id, type)}/roles";
		this._logger.RequestingStaff(id, type);
		var roles = await this._httpClient.GetFromJsonAsync(url, JsonSGContext.Default.ListRole, cancellationToken);
		roles!.RemoveAll(x => x.Person is null);
		roles.TrimExcess();
		return roles;
	}

	public Task<UserInfo> GetUserInfoAsync(uint userId, CancellationToken cancellationToken = default)
	{
		var url = $"{Constants.BaseUsersApiUrl}/{userId}/info";
		this._logger.RequestingUserInfo(userId);
		return this._httpClient.GetFromJsonAsync(url, JsonSGContext.Default.UserInfo, cancellationToken)!;
	}

	public async Task<IReadOnlyList<UserAchievement>> GetUserAchievementsAsync(uint userId, CancellationToken cancellationToken = default)
	{
		const string url = $"{Constants.BaseApiUrl}/achievements";
		this._logger.RequestingUserAchievements(userId);
		using var rm = new HttpRequestMessage(HttpMethod.Get, url)
		{
			Content = new MultipartFormDataContent
			{
				#pragma warning disable CA2000
				// Call System.IDisposable.Dispose on object created by 'new StringContent("1")' before all references to it are out of scope
				{ new StringContent(userId.ToString("D", CultureInfo.InvariantCulture)), "user_id" },
				#pragma warning restore CA2000
			},
		};
		using var response = await this._httpClient.SendAsync(rm, HttpCompletionOption.ResponseContentRead, cancellationToken);
		var achievements = (await response.Content.ReadFromJsonAsync(JsonSGContext.Default.UserAchievementArray, cancellationToken))!;
		var r = new List<UserAchievement>(achievements.Length);
		foreach (var userAchievement in achievements.Where(x => x is { Level: > 0 }).GroupBy(x => x.Id, StringComparer.Ordinal))
		{
			r.Add(new UserAchievement(userAchievement.Key, userAchievement.Max(x => x.Level)));
		}

		return r;
	}
}