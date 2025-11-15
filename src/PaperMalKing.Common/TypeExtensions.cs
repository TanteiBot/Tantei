// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PaperMalKing.Common;

[SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "False positive")]
[SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don\'t access instance data should be static", Justification = "False positive")]
public static partial class TypeExtensions
{
	[GeneratedRegex("<.*?>", RegexOptions.Compiled, matchTimeoutMilliseconds: 1000/*1s*/)]
	private static partial Regex HtmlRegex { get; }

	extension(string? value)
	{
		public string StripHtml() => value is null ? "" : HtmlRegex.Replace(value, string.Empty);

		public string? ToSentenceCase(CultureInfo cultureInfo)
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

		public string ToFirstCharUpperCase()
		{
			if (value is null)
			{
				return "";
			}

			if (char.IsUpper(value, 0))
			{
				return value;
			}

			return string.Create(value.Length, value, static (span, s) =>
			{
				span[0] = char.ToUpperInvariant(s[0]);
				s.AsSpan(1).CopyTo(span[1..]);
			});
		}
	}

	// This was not moved to extension because of
	// https://github.com/dotnet/roslyn/issues/80024
	public static bool HasAllFlags<TEnum>(this TEnum @enum, params TEnum[] flags)
		where TEnum : unmanaged, Enum
	{
		var result = true;

		foreach (var flag in flags)
		{
			result = result && @enum.HasFlag(flag);
		}

		return result;
	}

	public static bool HasAnyFlag<TEnum>(this TEnum @enum, params TEnum[] flags)
		where TEnum : unmanaged, Enum
	{
		var result = false;

		foreach (var flag in flags)
		{
			result = result || @enum.HasFlag(flag);

			if (result)
			{
				return result;
			}
		}

		return result;
	}

	public static string GetFullMessage(this Exception ex)
	{
		if (ex.InnerException is null)
		{
			return ex.Message;
		}

		return GetMessage(ex).JoinToString(";\n");

		static IEnumerable<string> GetMessage(Exception exception)
		{
			while (true)
			{
				if (!string.IsNullOrWhiteSpace(exception.Message))
				{
					yield return exception.Message;
				}

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