using System;
using System.Net;
using System.Net.Http;
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
		private readonly LogDelegate _log;
		private readonly ClockService _clock;
		private readonly RateLimiter _rateLimiter;

		private readonly HttpClient _httpClient;

		private const string LogName = "MalFeedReader";

		public FeedReader(LogDelegate log, ClockService clock, BotMalConfig malConfig)
		{
			this._log = log;
			this._clock = clock;
			this._rateLimiter =
				new RateLimiter(new RateLimit(malConfig.RateLimit.RequestsCount, TimeSpan.FromMilliseconds(malConfig.RateLimit.TimeConstraint)),
					this._clock, "MalRateLimiter", this._log);
			this._httpClient = HttpProvider.GetHttpClient(TimeSpan.FromMilliseconds(malConfig.Timeout));
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
					// #if DEBUG
					// this.Log(LogLevel.Debug, LogName, "Accessing MAL", this._clock.Now);
					// #endif
					response = await this._httpClient.GetAsync(url);
					var statusCode = (int)response.StatusCode;
					if (response.StatusCode == HttpStatusCode.TooManyRequests)
					{
						this._log(LogLevel.Warning, LogName,
							"Got ratelimited by Mal while trying to read rss feed, waiting 10s and retrying",
							this._clock.Now);
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