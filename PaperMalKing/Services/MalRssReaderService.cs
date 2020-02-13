using System;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using DSharpPlus;
using PaperMalKing.Utilities;

namespace PaperMalKing.Services
{
    public class MalRssReaderService
    {
        private long _lastRequestDate;

        private const string LogName = "MalRssReader";

        private LogDelegate Log;

        public MalRssReaderService(DiscordClient client)
        {
            this.Log = client.DebugLogger.LogMessage;
            this._lastRequestDate = DateTimeOffset.Now.Subtract(TimeSpan.FromSeconds(10)).ToUnixTimeMilliseconds();
        }

        public async Task<Feed> ReadFeedAsync(string url)
        {
            var timePassed = DateTimeOffset.Now.ToUnixTimeMilliseconds() - this._lastRequestDate;
            if (timePassed < 2000)
            {
                var delay = (int)(2000 - timePassed);
                this.Log(LogLevel.Debug, LogName, $"Waiting {delay} ms before reading next Rss feed", DateTime.Now);
                await Task.Delay(delay);
            }

            this._lastRequestDate = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            return await FeedReader.ReadAsync(url);

        }
    }
}
