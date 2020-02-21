using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DSharpPlus;
using PaperMalKing.MyAnimeList.Exceptions;
using PaperMalKing.Utilities;

namespace PaperMalKing.MyAnimeList.FeedReader
{
    public sealed class FeedReader
    {
        private long _lastRequestDate;

        private readonly LogDelegate Log;

        private readonly HttpClient _httpClient;

        private const string LogName = "MalFeedReader";

        public FeedReader(LogDelegate log)
        {
            this.Log = log;
            this._httpClient = new HttpClient();
            this._lastRequestDate = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        private async Task<HttpResponseMessage> MakeRequestAsync(string url)
        {
            var millisecondsPassed = DateTimeOffset.Now.ToUnixTimeMilliseconds() - this._lastRequestDate;
            if (millisecondsPassed < 2000)
            {
                var delay = (int)(2000 - millisecondsPassed);
                this.Log(LogLevel.Debug, LogName, $"Waiting {delay}ms before reading next rss feed.", DateTime.Now);
                await Task.Delay(delay);
            }

            HttpResponseMessage response;
            bool tryAgain;
            do
            {
                tryAgain = false;
                response = await this._httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    this._lastRequestDate = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                }
                else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    this.Log(LogLevel.Warning, LogName,
                        "Got ratelimited by Mal while trying to read rss feed, waiting 10s and retrying", DateTime.Now);
                    tryAgain = true;
                    response.Dispose();
                    await Task.Delay(TimeSpan.FromSeconds(10));
                }
            } while (tryAgain);

            return response;
        }
        

        public async Task<Feed> ReadFeedAsync(string url)
        {
            var response = await this.MakeRequestAsync(url);
            if (response.IsSuccessStatusCode)
            {
                using (var xml = await response.Content.ReadAsStreamAsync())
                {
                    var xmlSerializer = new XmlSerializer(typeof(Feed));
                    Feed feed;
                    try
                    {
                        feed = xmlSerializer.Deserialize(xml) as Feed;
                    }
                    catch (Exception ex)
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
            if(statusCode == HttpStatusCode.NotFound)
                throw new MalRssException(url, RssLoadResult.NotFound);
            if(statusCode == HttpStatusCode.GatewayTimeout)
                throw new MalRssException(url,RssLoadResult.GatewayTimeout);
            if(statusCode == HttpStatusCode.ServiceUnavailable)
                throw new MalRssException(url, RssLoadResult.ServiceUnavailable);
            if (statusCode == HttpStatusCode.BadGateway)
                throw new MalRssException(url, RssLoadResult.BadGateway);
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