// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using PaperMalKing.Common.Options;
using PaperMalKing.Common.RateLimiters;

namespace PaperMalKing.Common;

public static partial class TypeExtensions
{
	[GeneratedRegex("<.*?>", RegexOptions.Compiled, matchTimeoutMilliseconds: 60000/*1m*/)]
	private static partial Regex HtmlRegex();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string Substring(this string original, string endOfSubstring, bool before)
	{
		var index = original.IndexOf(endOfSubstring, StringComparison.OrdinalIgnoreCase);
		var result = before ? original.Substring(0, index) : original.Substring(index + 1, original.Length - index - 1);
		return result;
	}

	public static string? ToSentenceCase(this string? value, CultureInfo cultureInfo)
	{
		if (string.IsNullOrEmpty(value) || value.Length <= 1)
			return value;

		value = value.ToLower(cultureInfo);
		for (var i = 0; i < value.Length; i++)
		{
			var ch = value[i];
			if (char.IsLetter(ch))
				return $"{char.ToUpper(ch, cultureInfo)}{value.Substring(i + 1)}";
		}

		return value;
	}

	public static string StripHtml(this string value) => HtmlRegex().Replace(value, string.Empty);

	public static RateLimiter<T> ToRateLimiter<T>(this IRateLimitOptions<T> rateLimitOptions)
	{
		var rateLimit = new RateLimit(rateLimitOptions.AmountOfRequests, rateLimitOptions.PeriodInMilliseconds);
		return RateLimiterFactory.Create<T>(rateLimit);
	}
}