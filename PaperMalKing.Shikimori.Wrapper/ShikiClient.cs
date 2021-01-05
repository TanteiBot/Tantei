using System.Net.Http;
using System.Net.Http.Json;
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
			var url = $"{Constants.BASE_USERS_API_URL}/{nickname}";

			this._logger.LogDebug("Requesting {@Nickname} profile.", nickname);
			// DOnT DISPOSE SINCE CACHED CONTENT WILL BE DISPOSED TOO
			var rm = new HttpRequestMessage(HttpMethod.Get, url)
			{
				Content = ContentCaching.UserCachedContent
			};


			using var response = await this._httpClient.SendAsync(rm, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
			return (await response!.Content.ReadFromJsonAsync<User>(null, cancellationToken))!;
		}

		internal async Task<Favourites> GetUserFavouritesAsync(ulong userId, CancellationToken cancellationToken = default)
		{
			this._logger.LogDebug("Requesting {@UuserId} favourites", userId);
			var url = $"{Constants.BASE_USERS_API_URL}/{userId.ToString()}/favourites";
			var favs = await this._httpClient.GetFromJsonAsync<Favourites>(url, cancellationToken);
			return favs!;
		}

		internal async Task<Paginatable<History[]>> GetUserHistoryAsync(ulong userId, uint page, byte limit,
																		CancellationToken cancellationToken = default)
		{
			var url = $"{Constants.BASE_USERS_API_URL}/{userId.ToString()}/history";
			limit = Constants.HISTORY_LIMIT < limit ? Constants.HISTORY_LIMIT : limit;
			this._logger.LogDebug("Requesting {@UserId} history. Page {@Page}", userId, page);
			// DOnT DISPOSE SINCE CACHED CONTENT WILL BE DISPOSED TOO
			var rm = new HttpRequestMessage(HttpMethod.Get, url)
			{
				Content = ContentCaching.UserHistoryCachedContent.GetOrAdd((page, limit), key => new()
				{
					{new StringContent(key.Item1.ToString()), "page"},
					{new StringContent(key.Item2.ToString()), "limit"}
				})
			};
			using var response = await this._httpClient.SendAsync(rm, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

			var data = (await response.Content.ReadFromJsonAsync<History[]>(null, cancellationToken))!;
			var hasNextPage = data.Length == limit + 1;
			return new(data, hasNextPage);
		}

		internal Task<UserInfo> GetUserInfo(ulong userId, CancellationToken cancellationToken = default)
		{
			var url = $"{Constants.BASE_USERS_API_URL}/{userId.ToString()}/info";
			return this._httpClient.GetFromJsonAsync<UserInfo>(url, cancellationToken)!;
		}

		// There is no usage for list since i changed my mind on how ShikimoriUpdate provider should work 
		//
		// internal async Task<Paginatable<TLe[]>> GetUserListAsync<TL, TLe, TLse>(ulong userId, uint page, string status = "",
		// 																				   CancellationToken cancellationToken = default)
		// 	where TL : struct, IListType where TLe : BaseListEntry<TLse> where TLse : BaseListSubEntry
		// {
		// 	var tl = new TL();
		// 	this._logger.LogDebug(
		// 		$"Requesting {userId.ToString()} {tl.ListEntryType.Humanize()} list. Page {page.ToString()}.{(status == "" ? "" : $"Status {status}")}");
		// 	var url = $"{Constants.BASE_USERS_API_URL}/{userId.ToString()}/{tl.ListType}";
		//
		// 	// DOnT DISPOSE SINCE CACHED CONTENT WILL BE DISPOSED TOO
		// 	var rm = new HttpRequestMessage(HttpMethod.Get, url)
		// 	{
		// 		Content = ContentCaching.UserListContent.GetOrAdd(new(page, status), key => new()
		// 		{
		// 			{new StringContent(key.Item1.ToString()), "page"},
		// 			{new StringContent(Constants.LIST_LIMIT.ToString()), "limit"},
		// 			{new StringContent(key.Item2), "status"}
		// 		})
		// 	};
		//
		// 	using var response = await this._httpClient.SendAsync(rm, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
		// 	var data = (await response.Content.ReadFromJsonAsync<TLe[]>(null, cancellationToken))!;
		// 	var hasNextPage = data.Length == Constants.LIST_LIMIT + 1;
		// 	return new(data, hasNextPage);
		// }
	}
}