// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;

namespace PaperMalKing.Common;

[SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:Prefix local calls with this", Justification = "False positive")]
[SuppressMessage("Minor Code Smell", "S2325:Methods and properties that don\'t access instance data should be static", Justification = "False positive")]
[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "Compiler error https://github.com/dotnet/sdk/issues/51716")]
public static partial class TypeExtensions
{
	[GeneratedRegex("<.*?>", RegexOptions.Compiled, matchTimeoutMilliseconds: 1000/*1s*/)]
	private static partial Regex HtmlRegex { get; }

	[GeneratedRegex(
		@"[ \t\r\n]*(?:\(|\[)?(?:Source\s*:|Written\s+by\s+MAL\s+Rewrite)[\s\S]*\z",
		RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant,
		matchTimeoutMilliseconds: 1000)]
	private static partial Regex SourceTailRegex { get; }

	extension(string? value)
	{
		public string StripHtml()
		{
			return value is null ? "" : HtmlRegex.Replace(value, string.Empty);
		}

		public string RemoveSourceTail()
		{
			return value is null ? "" : SourceTailRegex.Replace(value, string.Empty);
		}

		public string? ToSentenceCase(CultureInfo cultureInfo)
		{
			if (string.IsNullOrWhiteSpace(value) || value.Length <= 1)
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

	extension<TEnum>(TEnum @enum)
		where TEnum : unmanaged, Enum
	{
		public bool HasAllFlags(params TEnum[] flags)
		{
			var result = true;

			foreach (var flag in flags)
			{
				result = result && @enum.HasFlag(flag);
			}

			return result;
		}

		public bool HasAnyFlag(params TEnum[] flags)
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
	}

	extension(HttpStatusCode hsc)
	{
		public bool IsServerSideError()
		{
			return hsc is HttpStatusCode.InternalServerError or HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable
				or HttpStatusCode.GatewayTimeout;
		}
	}

	extension(DiscordEmbedBuilder eb)
	{
		public DiscordEmbedBuilder AddFieldIfPresent(string name, string? value, bool inline = false)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(name);

			if (string.IsNullOrWhiteSpace(value))
			{
				return eb;
			}

			return eb.AddField(name, value, inline);
		}
	}
}