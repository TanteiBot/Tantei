using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
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

		public static IList<T> Shuffle<T>(this IList<T> list)
		{
			using var provider = new RNGCryptoServiceProvider();
			var n = list.Count;
			Span<byte> box = stackalloc byte[sizeof(int)];
			while (n > 1)
			{
				provider.GetBytes(box);
				ReadOnlySpan<byte> readonlyBox = box;
				var bit = BitConverter.ToInt32(readonlyBox);
				var k = Math.Abs(bit) % n;
				n--;
				var value = list[k];
				list[k] = list[n];
				list[n] = value;
			}

			return list;
		}

		public static (IReadOnlyList<T> AddedValues, IReadOnlyList<T> RemovedValues) GetDifference<T>(
			this IReadOnlyList<T> original, IReadOnlyList<T> resulting)
		{
			var originalHs = new HashSet<T>(original);
			var resultingHs = new HashSet<T>(resulting);
			originalHs.ExceptWith(resulting);
			resultingHs.ExceptWith(original);
			var added = resultingHs.ToArray() ?? Array.Empty<T>();
			var removed = originalHs.ToArray() ?? Array.Empty<T>();
			return (added, removed);
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