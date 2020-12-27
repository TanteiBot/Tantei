using System;
using System.Text.RegularExpressions;

namespace PaperMalKing.MyAnimeList.Wrapper.Parsers
{
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
}