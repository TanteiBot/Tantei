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

using System.Text.RegularExpressions;

namespace PaperMalKing.MyAnimeList.Wrapper.Parsers;

internal static class MalDateTimeParser
{
	private static readonly Regex AgoRegex = new("(?<number>[0-9]+) (?<time>[^\\s]+) (ago)", RegexOptions.Compiled);


	internal static DateTimeOffset? ParseOrDefault(string value)
	{
		var agoRegexMatch = AgoRegex.Match(value);
		return agoRegexMatch.Success ? AgoParse(agoRegexMatch) : null;
	}

	private static DateTimeOffset? AgoParse(Match agoRegexMatch)
	{
		var now = DateTimeOffset.UtcNow;
		var number = int.Parse(agoRegexMatch.Groups["number"].Value);
		return agoRegexMatch.Groups["time"].Value switch
		{
			"seconds" => now.AddSeconds(-number),
			"minutes" => now.AddMinutes(-number),
			"hours"   => now.AddHours(-number),
			_         => null
		};
	}
}