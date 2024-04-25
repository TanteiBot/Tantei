// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using PaperMalKing.Common.Options;
using PaperMalKing.Common.RateLimiters;

namespace PaperMalKing.Common;

public static partial class TypeExtensions
{
	[GeneratedRegex("<.*?>", RegexOptions.Compiled, matchTimeoutMilliseconds: 60000/*1m*/)]
	private static partial Regex HtmlRegex();

	public static string? ToSentenceCase(this string? value, CultureInfo cultureInfo)
	{
		if (string.IsNullOrEmpty(value) || value.Length <= 1)
		{
			return value;
		}

		value = value.ToLower(cultureInfo);
		for (var i = 0; i < value.Length; i++)
		{
			var ch = value[i];
			if (char.IsLetter(ch))
			{
				return $"{char.ToUpper(ch, cultureInfo)}{value[(i + 1)..]}";
			}
		}

		return value;
	}

	public static string StripHtml(this string value) => HtmlRegex().Replace(value, string.Empty);

	public static RateLimiter<T> ToRateLimiter<T>(this IRateLimitOptions<T> rateLimitOptions)
	{
		var rateLimit = new RateLimitValue(rateLimitOptions.AmountOfRequests, rateLimitOptions.PeriodInMilliseconds);
		return RateLimiterFactory.Create<T>(rateLimit);
	}

	public static string ToFirstCharUpperCase(this string? str)
	{
		if (str is null)
		{
			return "";
		}

		if (char.IsUpper(str, 0))
		{
			return str;
		}

		return string.Create(str.Length, str, (span, s) =>
		{
			span[0] = char.ToUpperInvariant(s[0]);
			s.AsSpan(1).CopyTo(span[1..]);
		});
	}

	public static string GetFullMessage(this Exception ex)
	{
		if (ex.InnerException is null)
		{
			return ex.Message;
		}

		return string.Join(";\n", GetMessage(ex).Where(x => !string.IsNullOrWhiteSpace(x)));

		static IEnumerable<string> GetMessage(Exception exception)
		{
			while (true)
			{
				yield return exception.Message;

				if (exception.InnerException is not null)
				{
					exception = exception.InnerException;
					continue;
				}

				break;
			}
		}
	}
}