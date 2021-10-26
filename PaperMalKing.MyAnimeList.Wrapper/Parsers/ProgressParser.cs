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

using System.Diagnostics.CodeAnalysis;
using PaperMalKing.MyAnimeList.Wrapper.Models.Progress;

namespace PaperMalKing.MyAnimeList.Wrapper.Parsers
{
	[SuppressMessage("Globalization", "CA1307")]
	internal static class ProgressParser
	{
		internal static GenericProgress Parse(string value)
		{
			value = value.Trim();
			if (value.Length == 0 || value == "?")
				return GenericProgress.Reprogressing;
			value = value.Replace("-", "").Replace(" ", "");
			if (TryParse<GenericProgress>(value, out var genericResult))
				return genericResult;
			if (TryParse<AnimeProgress>(value, out var animeResult))
				return animeResult.ToGeneric();
			if (TryParse<MangaProgress>(value, out var mangaResult))
				return mangaResult.ToGeneric();
			return GenericProgress.Unknown;
		}

		private static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : unmanaged, Enum =>
			Enum.TryParse(value, true, out result);
	}
}