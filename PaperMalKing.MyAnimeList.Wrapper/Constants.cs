// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
namespace PaperMalKing.MyAnimeList.Wrapper;

internal static class Constants
{
	internal const string BASE_URL = "https://myanimelist.net";

	internal const string USER_AVATAR = "https://cdn.myanimelist.net/images/userimages/";

	internal const string PROFILE_URL = BASE_URL + "/profile/";

	internal const string COMMENTS_URL = BASE_URL + "/comments.php?id=";

	internal const string RSS_ANIME_URL = "https://myanimelist.net/rss.php?type=rw&u=";

	internal const string RSS_MANGA_URL = "https://myanimelist.net/rss.php?type=rm&u=";

	internal const string ANIME_LIST_URL = BASE_URL + "/animelist/";

	internal const string MANGA_LIST_URL = BASE_URL + "/mangalist/";

	internal const string LATEST_LIST_UPDATES = "/load.json?order=5&status=7";

	// Discord doesn't support .ico formats in footer icons
	internal const string FAV_ICON = "https://pbs.twimg.com/profile_images/1190380284295950339/Py6XnxvH_200x200.jpg";

	internal const string DOT = "&middot;";
}