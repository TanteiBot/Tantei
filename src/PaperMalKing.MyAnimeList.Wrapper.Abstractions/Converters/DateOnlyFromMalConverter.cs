// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Converters;

public sealed class DateOnlyFromMalConverter : JsonConverter<DateOnly?>
{
	/// <summary>
	/// yyyy-mm-dd
	/// </summary>
	private const int MaxDateLength = 10;

	private static readonly string[] Formats = { "yyyy-MM-dd", "yyyy-MM", "yyyy", "yyyy-M-dd", "yyyy-M-d", "yyyy-M", };

	public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType is not JsonTokenType.String)
		{
			throw new JsonException("MyAnimeList's Date cant be converted from non-string");
		}

		scoped Span<char> buffer = stackalloc char[MaxDateLength];
		var charsWritten = reader.CopyString(buffer);
		buffer = buffer.Slice(0, charsWritten);
		foreach (var t in Formats)
		{
			if (DateOnly.TryParseExact(buffer, t, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out var result))
			{
				return result;
			}
		}

		throw new JsonException("Date doesnt match any of the formats specified");
	}

	public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
	{
		// NOOP
	}
}