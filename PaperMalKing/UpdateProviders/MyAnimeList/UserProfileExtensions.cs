using System;
using PaperMalKing.Database.Models.MyAnimeList;

namespace PaperMalKing.UpdateProviders.MyAnimeList
{
	public static class UserProfileExtensions
	{
		public static string GetAvatarUrl(this User profile)
		{
			var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
			return $"{MalConstants.BASE_USER_AVATAR_URL}{profile.UserId.ToString()}.jpg?t={now.ToString()}";
		}

		public static string GetProfileUrl(this User profile) => $"{MalConstants.BASE_PROFILE_URL}{profile.Username}";

		public static string GetAnimeRssFeedUrl(this User profile) =>
			$"{MalConstants.BASE_USER_ANIME_RSS_FEED}{profile.Username}";

		public static string GetMangaRssFeedUrl(this User profile) =>
			$"{MalConstants.BASE_USER_MANGA_RSS_FEED}{profile.Username}";
	}
}