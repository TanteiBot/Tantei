using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DSharpPlus;
using PaperMalKing.Data;
using PaperMalKing.MyAnimeList.Exceptions;
using PaperMalKing.Services;
using PaperMalKing.Utilities;

namespace PaperMalKing.MyAnimeList.FeedReader
{
	/// <summary>
	/// RSS reader for MyAnimeList anime and manga rss feeds
	/// </summary>
	public sealed class FeedReader
	{
		private readonly LogService _logService;
		private readonly RateLimiter _rateLimiter;

		private readonly HttpClient _httpClient;

		private const string LogName = "MalFeedReader";

		public FeedReader(BotConfig config, LogService logService, ClockService clock, HttpClient httpClient)
		{
			var malConfig = config.MyAnimeList;
			this._logService = logService;
			this._rateLimiter =
				new RateLimiter(new RateLimit(malConfig.RateLimit.RequestsCount, TimeSpan.FromMilliseconds(malConfig.RateLimit.TimeConstraint)),
					 "MalRateLimiter", clock, this._logService);
			this._httpClient = httpClient;
		}

		private HttpRequestMessage PrepareHttpRequest(string url)
		{
			var request = new HttpRequestMessage(HttpMethod.Get, url);
			request.Headers.UserAgent.TryParseAdd(
				"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:76.0) Gecko/20100101 Firefox/76.0");
			return request;
		}

		private async Task<HttpResponseMessage> MakeRequestAsync(string url)
		{

			HttpResponseMessage response = null;
			bool tryAgain;
			do
			{
				tryAgain = false;
				try
				{
					await this._rateLimiter.GetTokenAsync();
					var request = this.PrepareHttpRequest(url);
					response = await this._httpClient.SendAsync(request);
					var statusCode = (int)response.StatusCode;
					if (response.StatusCode == HttpStatusCode.TooManyRequests)
					{
						this._logService.Log(LogLevel.Warning, LogName,
							"Got ratelimited by Mal while trying to read rss feed, waiting 10s and retrying");
						tryAgain = true;
						response.Dispose();
						await Task.Delay(TimeSpan.FromSeconds(10));
					}
					else if (response.StatusCode == HttpStatusCode.Forbidden)
						throw new MalRssException(url, RssLoadResult.Forbidden);
					else if (response.StatusCode == HttpStatusCode.NotFound)
						throw new MalRssException(url, RssLoadResult.NotFound);
					else if (statusCode >= 500 && statusCode < 600)
					{
						throw new ServerSideException(url,
							$"Encountered serverside issue while accessing {url} with status code {statusCode}");
					}
				}
				catch (TaskCanceledException)
				{
					throw new ServerSideException(url, "Waited too long for accessing feed");
				}
			} while (tryAgain);

			return response;
		}


		public async Task<Feed> ReadFeedAsync(string url)
		{
			var response = await this.MakeRequestAsync(url);
			if (response.IsSuccessStatusCode)
			{
				await using (var xml = await response.Content.ReadAsStreamAsync())
				{
					var xmlSerializer = new XmlSerializer(typeof(Feed));
					Feed feed;
					try
					{
						feed = xmlSerializer.Deserialize(xml) as Feed;
					}
					catch
					{
						throw new MalRssException(url, RssLoadResult.EmptyList);
					}
					finally
					{
						response.Dispose();
					}

					feed?.TryFillDateTime();
					return feed;
				}
			}

			var statusCode = response.StatusCode;
			response.Dispose();

			if (statusCode == HttpStatusCode.Forbidden)
				throw new MalRssException(url, RssLoadResult.Forbidden);
			if (statusCode == HttpStatusCode.NotFound)
				throw new MalRssException(url, RssLoadResult.NotFound);
			int statusCodeInt = (int) statusCode;
			if(statusCodeInt == 418) // Since MAL returns 418 when requesting to private list's rss feed
				throw new MalRssException(url, RssLoadResult.ImTeapot);
			throw new Exception($"Mal returned {statusCode} when tried to access `{url}`");
		}

		public async Task<RssLoadResult> GetRssFeedLoadResult(string url)
		{
			using (var response = await this.MakeRequestAsync(url))
			{
				var statusCode = response.StatusCode;
				if (statusCode != HttpStatusCode.OK)
				{
					if (statusCode == HttpStatusCode.Forbidden)
						return RssLoadResult.Forbidden;
					if (statusCode == HttpStatusCode.NotFound)
						return RssLoadResult.NotFound;
					return RssLoadResult.Unknown;
				}

				var content = await response.Content.ReadAsStringAsync();
				return !content.StartsWith("<?xml") ? RssLoadResult.EmptyList : RssLoadResult.Ok;
			}
		}
	}
}