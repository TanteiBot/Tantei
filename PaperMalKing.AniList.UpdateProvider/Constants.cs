using DSharpPlus.Entities;

namespace PaperMalKing.AniList.UpdateProvider
{
    internal static class Constants
    {
        public const string NAME = "AniList";

        public const string ICON_URL = "https://anilist.co/img/icons/android-chrome-512x512.png";

        /// <summary>
        /// Completed
        /// </summary>
        public static readonly DiscordColor AniListGreen = new("#7bd555");

        /// <summary>
        /// Planned
        /// </summary>
        public static readonly DiscordColor AniListOrange = new("#f79a63");

        /// <summary>
        /// Dropped
        /// </summary>
        public static readonly DiscordColor AniListRed = new("#e85d75");

        /// <summary>
        /// Paused
        /// </summary>
        public static readonly DiscordColor AniListPeach = new("#fa7a7a");

        /// <summary>
        /// Current
        /// </summary>
        public static readonly DiscordColor AniListBlue = new("#3db4f2");
    }
}