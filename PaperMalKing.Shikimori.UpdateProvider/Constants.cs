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

namespace PaperMalKing.Shikimori.UpdateProvider;

internal static class Constants
{
	public const string NAME = "Shikimori";

	public const string ICON_URL = "https://shikimori.one/favicons/opera-icon-228x228.png";

	public static readonly DiscordColor ShikiGreen = new("#419541");

	public static readonly DiscordColor ShikiRed = new("#FC575E");

	public static readonly DiscordColor ShikiGrey = new("#7b8084");

	public static readonly DiscordColor ShikiBlue = new("#176093");
}