#region LICENSE
// PaperMalKing.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

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