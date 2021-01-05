using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common.Converters;
using PaperMalKing.MyAnimeList.Wrapper.Models;
using PaperMalKing.MyAnimeList.Wrapper.Models.List;
using PaperMalKing.MyAnimeList.Wrapper.Models.List.Types;
using PaperMalKing.MyAnimeList.Wrapper.Models.Rss;
using PaperMalKing.MyAnimeList.Wrapper.Models.Rss.Types;
using PaperMalKing.MyAnimeList.Wrapper.Parsers;

namespace PaperMalKing.MyAnimeList.Wrapper
{
	public sealed class MyAnimeListClient
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<MyAnimeListClient> _logger;

		private readonly JsonSerializerOptions _jsonSerializerOptions = new()
		{
			Converters =
			{
				new JsonNumberToStringConverter(),
				new JsonToBoolConverter()
			}
		};

		private readonly XmlSerializer _xmlSerializer;

		internal MyAnimeListClient(ILogger<MyAnimeListClient> logger, HttpClient httpClient)
		{
			this._logger = logger;
			this._httpClient = httpClient;
			this._xmlSerializer = new(typeof(Feed));
		}

		private async Task<HttpResponseMessage> GetAsync(string url, CancellationToken cancellationToken = default)
		{
			var response = await this._httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
			return response.EnsureSuccessStatusCode();
		}

		private async Task<HtmlNode> GetAsHtmlAsync(string url, CancellationToken cancellationToken = default)
		{
			using var response = await this.GetAsync(url, cancellationToken);
			var doc = new HtmlDocument();
			await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
			doc.Load(stream);
			return doc.DocumentNode;
		}

		internal async Task<IEnumerable<FeedItem>> GetRecentRssUpdatesAsync<TR>(string username, CancellationToken cancellationToken = default)
			where TR : struct, IRssFeedType
		{
			var rssType = new TR();
			username = WebUtility.UrlEncode(username);
			var url = $"{rssType.Url}{username}";
			using var response = await this.GetAsync(url, cancellationToken);

			await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
			Feed? feed;
			try
			{
				feed = (Feed?) this._xmlSerializer.Deserialize(stream);
			}
			catch
			{
				return Enumerable.Empty<FeedItem>();
			}

			return feed?.Items ?? Enumerable.Empty<FeedItem>();
		}

		internal async Task<User> GetUserAsync(string username, CancellationToken cancellationToken = default)
		{
			this._logger.LogDebug("Requesting {@Username} profile", username);
			username = WebUtility.UrlEncode(username);
			var requestUrl = Constants.PROFILE_URL + username;
			var htmlNode = await this.GetAsHtmlAsync(requestUrl, cancellationToken);
			return UserProfileParser.Parse(htmlNode);
		}

		internal async Task<string> GetUsernameAsync(ulong id, CancellationToken cancellationToken = default)
		{
			var url = $"{Constants.COMMENTS_URL}{id.ToString()}";
			this._logger.LogDebug("Requesting username by id {@Id}", id);
			var htmlNode = await this.GetAsHtmlAsync(url, cancellationToken);
			return CommentsParser.Parse(htmlNode);
		}

		internal async Task<IReadOnlyList<TE>> GetLatestListUpdatesAsync<TE, TListType>(string username,
																						CancellationToken cancellationToken = default)
			where TE : class, IListEntry where TListType : struct, IListType<TE>
		{
			var tl = new TListType();
			this._logger.LogDebug("Requesting {@Username} {@Type} list", username, tl.ListEntryType);

			username = WebUtility.UrlEncode(username);
			var url = tl.LatestUpdatesUrl(username);
			using var response = await this.GetAsync(url, cancellationToken);

			await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
			var updates = await JsonSerializer.DeserializeAsync<TE[]>(stream, this._jsonSerializerOptions, cancellationToken);
			return updates ?? Array.Empty<TE>();
		}
	}
}