// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
namespace PaperMalKing.MyAnimeList.Wrapper;

internal static class Constants
{
	public const string BASE_URL = "https://myanimelist.net";

	public const string USER_AVATAR = "https://cdn.myanimelist.net/images/userimages/";

	public const string PROFILE_URL = BASE_URL + "/profile/";

	public const string COMMENTS_URL = BASE_URL + "/comments.php?id=";

	public const string BASE_OFFICIAL_API_URL = "https://api.myanimelist.net/v2";

	// Discord doesn't support .ico formats in footer icons
	public const string FAV_ICON = "https://pbs.twimg.com/profile_images/1190380284295950339/Py6XnxvH_200x200.jpg";

	public const string DOT = "&middot;";
}