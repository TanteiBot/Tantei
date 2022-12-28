// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Text.RegularExpressions;

namespace PaperMalKing.MyAnimeList.Wrapper.Parsers;

internal static partial class MalDateTimeParser
{
	[GeneratedRegex("(?<number>[0-9]+) (?<time>[^\\s]+) (ago)", RegexOptions.Compiled, matchTimeoutMilliseconds: 30000 /*30s*/)]
	private static partial Regex AgoRegex();

	internal static DateTimeOffset? ParseOrDefault(string value)
	{
		var agoRegexMatch = AgoRegex().Match(value);
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
			"hours" => now.AddHours(-number),
			_ => null
		};
	}
}