using System;

namespace PaperMalKing.MyAnimeList.Exceptions
{
    /// <summary>
    /// Exception that represents error while accessing MyAnimeList's user list
    /// </summary>
    sealed class MalRssException : Exception
    {
        /// <summary>
        /// Url at which error happened
        /// </summary>
        public readonly string Url;

        /// <summary>
        /// MyAnimeList username of user
        /// </summary>
        public readonly string Username;

        /// <summary>
        /// Type of user's list on MyAnimeList
        /// </summary>
        public readonly EntityType ListType;

        /// <summary>
        /// Reason why error happened
        /// </summary>
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
