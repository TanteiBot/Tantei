using System;

namespace PaperMalKing.MyAnimeList.Exceptions
{
    sealed class MalRssException : Exception
    {
        public readonly string Url;

        public readonly string Username;

        public readonly EntityType ListType;

        public readonly RssLoadResult Reason;

        public MalRssException(string url, RssLoadResult reason)
        {
            this.Url = url;
            this.Reason = reason;
            this.ListType = url.Contains("type=rw&=") ? EntityType.Anime : EntityType.Manga;
            var index = url.LastIndexOf("u=");
            this.Username = url.Substring(index + 2);
        }
    }
}
