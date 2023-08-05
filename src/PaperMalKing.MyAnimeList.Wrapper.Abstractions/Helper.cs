// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Globalization;
using System.Text.RegularExpressions;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions;

public static partial class Helper
{
	[GeneratedRegex(@"(?<=\/)(?<id>\d+)(?=\/)", RegexOptions.Compiled, matchTimeoutMilliseconds: 20000 /*20s*/)]
	private static partial Regex IdFromUrlRegex();

	public static uint ExtractIdFromMalUrl(string url) => uint.Parse(IdFromUrlRegex().Match(url).Groups["id"].Value, NumberFormatInfo.InvariantInfo);
}