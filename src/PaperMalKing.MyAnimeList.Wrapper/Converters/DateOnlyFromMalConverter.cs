// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Converters;

internal sealed class DateOnlyFromMalConverter : JsonConverter<DateOnly?>
{
	private const int MaxDateLength = 10; // yyyy-mm-dd

	private static readonly IReadOnlyList<string> Formats = new[] { "yyyy-MM-dd", "yyyy-MM", "yyyy", "yyyy-M-dd", "yyyy-M-d", "yyyy-M", };

	public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType is not JsonTokenType.String)
		{
			throw new JsonException("MyAnimeList's Date cant be converted from non-string");
		}

		scoped Span<char> buffer = stackalloc char[MaxDateLength];
		var charsWritten = reader.CopyString(buffer);
		buffer = buffer.Slice(0, charsWritten);
		for (var i = 0; i < Formats.Count; i++)
		{
			if (DateOnly.TryParseExact(buffer, Formats[i], out var result))
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