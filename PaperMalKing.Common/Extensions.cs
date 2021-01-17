using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common.Options;
using PaperMalKing.Common.RateLimiter;

namespace PaperMalKing.Common
{
	public static class Extensions
	{
		public static string ToFixedWidth(this string s, int newLength)
		{
			if (s.Length < newLength)
				return s.PadRight(newLength);

			if (s.Length > newLength)
				return s.Substring(0, newLength);

			return s;
		}

		public static string FirstCharToUpper(this string input) => input switch
		{
			null => throw new ArgumentNullException(nameof(input)),
			""   => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
			_    => input.First().ToString().ToUpper() + input.Substring(1)
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static string Substring(this string original, string endOfSubstring, bool before)
		{
			var index = original.IndexOf(endOfSubstring, StringComparison.InvariantCultureIgnoreCase);
			var result = before ? original.Substring(0, index) : original.Substring(index+1, original.Length - index - 1);
			return result;
		}

		public static string? ToSentenceCase(this string? value, CultureInfo? cultureInfo = null)
		{
			if (string.IsNullOrEmpty(value) || value.Length <= 1)
				return value;

			value = value.ToLower(cultureInfo);
			for (var i = 0; i < value.Length; i++)
			{
				var ch = value[i];
				if (char.IsLetter(ch))
					return $"{char.ToUpper(ch).ToString()}{value.Substring(i + 1)}";
			}

			return value;
		}
		
		public static string SplitByCapitalLetter(this string s) =>
			string.Join(' ', Regex.Split(s, @"(?<!^)(?=[A-Z])", RegexOptions.Compiled)).FirstCharToUpper();

		public static IRateLimiter<T> ToRateLimiter<T>(this IRateLimitOptions<T> rateLimitOptions, ILogger<IRateLimiter<T>>? logger)
		{
			var rateLimit = new RateLimit(rateLimitOptions.AmountOfRequests, rateLimitOptions.PeriodInMilliseconds);
			return RateLimiterFactory.Create(rateLimit, logger);
		}
	}
}