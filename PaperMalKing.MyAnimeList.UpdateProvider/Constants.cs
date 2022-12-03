#region LICENSE
// PaperMalKing.
// Copyright (C) 2021-2022 N0D4N
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

namespace PaperMalKing.UpdatesProviders.MyAnimeList
{
	internal static class Constants
	{
		internal const string Name = "MyAnimeList";

		internal static readonly DiscordColor MalGreen = new("#2db039");

		internal static readonly DiscordColor MalBlue = new("#26448f");

		internal static readonly DiscordColor MalYellow = new("#f9d457");

		internal static readonly DiscordColor MalRed = new("#a12f31");

		internal static readonly DiscordColor MalGrey = new("#c3c3c3");

		internal static readonly DiscordColor MalBlack = DiscordColor.NotQuiteBlack;
	}
}