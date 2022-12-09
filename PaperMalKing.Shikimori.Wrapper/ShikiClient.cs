#region LICENSE
// PaperMalKing.
// Copyright (C) 2021-2022 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PaperMalKing.Shikimori.Wrapper.Models;

namespace PaperMalKing.Shikimori.Wrapper
{
	public sealed class ShikiClient
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<ShikiClient> _logger;

		public ShikiClient(HttpClient httpClient, ILogger<ShikiClient> logger)
		{
			this._httpClient = httpClient;
			this._logger = logger;
		}

		internal async Task<User> GetUserAsync(string nickname, CancellationToken cancellationToken = default)
		{
			this._logger.LogDebug("Requesting {@Nickname} profile", nickname);

			nickname = WebUtility.UrlEncode(nickname);
			var url = $"{Constants.BASE_USERS_API_URL}/{nickname}";

			using var rm = new HttpRequestMessage(HttpMethod.Get, url)
			{
				Content = new MultipartFormDataContent()
				{
					{ new StringContent("1"), "is_nickname" }
				}
			};

			using var response = await this._httpClient.SendAsync(rm, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
			return (await response!.Content.ReadFromJsonAsync<User>((JsonSerializerOptions?)null, cancellationToken).ConfigureAwait(false))!;
		}

		internal async Task<Favourites> GetUserFavouritesAsync(ulong userId, CancellationToken cancellationToken = default)
		{
			this._logger.LogDebug("Requesting {@UserId} favourites", userId);
			var url = $"{Constants.BASE_USERS_API_URL}/{userId}/favourites";
			var favs = await this._httpClient.GetFromJsonAsync<Favourites>(url, cancellationToken).ConfigureAwait(false);
			return favs!;
		}

		internal async Task<Paginatable<History[]>> GetUserHistoryAsync(ulong userId, uint page, byte limit, HistoryRequestOptions options, 
																		CancellationToken cancellationToken = default)
		{
			var url = $"{Constants.BASE_USERS_API_URL}/{userId}/history";
			limit = Constants.HISTORY_LIMIT < limit ? Constants.HISTORY_LIMIT : limit;
			this._logger.LogDebug("Requesting {@UserId} history. Page {@Page}", userId, page);

			#pragma warning disable CA2000
			using var content = new MultipartFormDataContent
			{
				{new StringContent(page.ToString()), "page"},
				{new StringContent(limit.ToString()), "limit"}
			};
			if (options != HistoryRequestOptions.Any) content.Add(new StringContent(options.ToString()), "target_type");
			#pragma warning restore CA2000

			using var rm = new HttpRequestMessage(HttpMethod.Get, url)
			{
				Content = content
			};
			using var response = await this._httpClient.SendAsync(rm, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

			var data = (await response.Content.ReadFromJsonAsync<History[]>((JsonSerializerOptions?)null, cancellationToken).ConfigureAwait(false))!;
			var hasNextPage = data.Length == limit + 1;
			return new(data, hasNextPage);
		}

		internal Task<UserInfo> GetUserInfoAsync(ulong userId, CancellationToken cancellationToken = default)
		{
			var url = $"{Constants.BASE_USERS_API_URL}/{userId}/info";
			return this._httpClient.GetFromJsonAsync<UserInfo>(url, cancellationToken)!;
		}
	}
}