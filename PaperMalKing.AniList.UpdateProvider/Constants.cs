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

using DSharpPlus.Entities;

namespace PaperMalKing.AniList.UpdateProvider
{
	internal static class Constants
	{
		public const string NAME = "AniList";

		public const string URL = "https://anilist.co";

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