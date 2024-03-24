// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Buffers.Text;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.UpdateProvider.Achievements;

public sealed class HexNumberJsonConverter : JsonConverter<int>
{
	private const int ByteBufferLength = 8;

	private const int MaxLengthLimit = 7;

	public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		Debug.Assert(reader.TokenType == JsonTokenType.String, "Must be a string");

		scoped Span<byte> bytes = stackalloc byte[ByteBufferLength];
		var bytesWritten = reader.CopyString(bytes);
		bytes = bytes[1..bytesWritten];

		if (Utf8Parser.TryParse(bytes, out int value, out _, 'X'))
		{
			return value;
		}

		scoped Span<char> chars = stackalloc char[MaxLengthLimit];
		var charsWritten = Encoding.UTF8.GetChars(bytes, chars);
		var hexValue = chars[..charsWritten];
		return int.Parse(hexValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
	}

	public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
	{
	}
}