using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.Extensions.Logging;
using PaperMalKing.Shikimori.Wrapper.Models;
using PaperMalKing.Shikimori.Wrapper.Models.List;

namespace PaperMalKing.Shikimori.Wrapper
{
	internal sealed class ShikiClient
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<ShikiClient> _logger;

		public ShikiClient(HttpClient httpClient, ILogger<ShikiClient> logger)
		{
			this._httpClient = httpClient;
			this._logger = logger;
		}

		private async Task<User> GetUserAsync(string identifier, bool getByNickname, CancellationToken cancellationToken = default)
		{
			var url = $"{Constants.BASE_USERS_API_URL}/{identifier}";

			this._logger.LogDebug($"Requesting {identifier} profile.");
			// DOnT DISPOSE SINCE CACHED CONTENT WILL BE DISPOSED TOO
			var rm = new HttpRequestMessage(HttpMethod.Get, url);
			if (getByNickname)
				rm.Content = ContentCaching.UserCachedContent;

			using var response = await this._httpClient.SendAsync(rm, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
			return (await response!.Content.ReadFromJsonAsync<User>(null, cancellationToken))!;
		}

		public Task<User> GetUserAsync(string username, CancellationToken cancellationToken = default) =>
			this.GetUserAsync(username, true, cancellationToken);

		public Task<User> GetUserAsync(ulong userId, CancellationToken cancellationToken = default) =>
			this.GetUserAsync(userId.ToString(), false, cancellationToken);

		public async Task<Favourites> GetUserFavouritesAsync(ulong userId, CancellationToken cancellationToken = default)
		{
			this._logger.LogDebug($"Requesting {userId.ToString()} favourites");
			var url = $"{Constants.BASE_USERS_API_URL}/favourites";
			return (await this._httpClient.GetFromJsonAsync<Favourites>(url, cancellationToken))!;
		}

		public async Task<Paginatable<IReadOnlyList<History>>> GetUserHistoryAsync(ulong userId, int page,
																				   CancellationToken cancellationToken = default)
		{
			var url = $"{Constants.BASE_USERS_API_URL}/{userId.ToString()}/history";

			this._logger.LogDebug($"Requesting {userId.ToString()} history. Page {page.ToString()}");
			// DOnT DISPOSE SINCE CACHED CONTENT WILL BE DISPOSED TOO
			var rm = new HttpRequestMessage(HttpMethod.Get, url)
			{
				Content = ContentCaching.UserHistoryCachedContent.GetOrAdd(page, key => new()
				{
					{new StringContent(key.ToString()), "page"},
					{new StringContent(Constants.HISTORY_LIMIT.ToString()), "limit"}
				})
			};
			using var response = await this._httpClient.SendAsync(rm, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

			var data = (await response.Content.ReadFromJsonAsync<IReadOnlyList<History>>(null, cancellationToken))!;
			var hasNextPage = data.Count == Constants.HISTORY_LIMIT + 1;
			return new(data, page, hasNextPage);
		}


		public async Task<Paginatable<IReadOnlyList<TLe>>> GetUserListAsync<TL, TLe, TLse>(ulong userId, int page, string status = "",
																						   CancellationToken cancellationToken = default)
			where TL : struct, IListType where TLe : BaseListEntry<TLse> where TLse : BaseListSubEntry
		{
			var tl = new TL();
			this._logger.LogDebug(
				$"Requesting {userId.ToString()} {tl.ListEntryType.Humanize()} list. Page {page.ToString()}.{(status == "" ? "" : $"Status {status}")}");
			var url = $"{Constants.BASE_USERS_API_URL}/{userId.ToString()}/{tl.ListType}";

			// DOnT DISPOSE SINCE CACHED CONTENT WILL BE DISPOSED TOO
			var rm = new HttpRequestMessage(HttpMethod.Get, url)
			{
				Content = ContentCaching.UserListContent.GetOrAdd(new(page, status), key => new()
				{
					{new StringContent(key.Item1.ToString()), "page"},
					{new StringContent(Constants.LIST_LIMIT.ToString()), "limit"},
					{new StringContent(key.Item2), "status"}
				})
			};

			using var response = await this._httpClient.SendAsync(rm, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
			var data = (await response.Content.ReadFromJsonAsync<IReadOnlyList<TLe>>(null, cancellationToken))!;
			var hasNextPage = data.Count == Constants.LIST_LIMIT + 1;
			return new(data, page, hasNextPage);
		}
	}
}