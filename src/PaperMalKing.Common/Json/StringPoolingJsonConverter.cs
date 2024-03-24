// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using CommunityToolkit.HighPerformance.Buffers;

namespace PaperMalKing.Common.Json;

/// <summary>
/// This converter tries to pool string from incoming json if possible.
/// It is intended to work on small strings only (smaller or equal to <see cref="MaxLengthLimit"/> chars)
/// If input string is escaped, or length in bytes exceed 256, it isn't pooled, and is just allocated.
/// </summary>
public sealed class StringPoolingJsonConverter : JsonConverter<string>
{
	private static readonly StringPool StringPool = new();

	public const int MaxLengthLimit = 128;

	public static string ReadStringOrGetFromPool(ref Utf8JsonReader reader) => ReadStringOrGetFromPool(ref reader, StringPool);

	/// <param name="reader">The reader.</param>
	/// <param name="stringPool">Pool to pool to or from value.</param>
	/// <remarks>
	/// Uses ref in order to not copy struct when invoking <see cref="Utf8JsonReader.GetString"/> in unhappy paths.
	/// </remarks>
	public static string ReadStringOrGetFromPool(scoped ref Utf8JsonReader reader, StringPool stringPool)
	{
		Debug.Assert(reader.TokenType == JsonTokenType.String, "Must be a string");
		if (reader.ValueIsEscaped || (reader.HasValueSequence ? reader.ValueSequence.Length : reader.ValueSpan.Length) > 256)
		{
			return reader.GetString()!;
		}

		scoped Span<char> chars = stackalloc char[MaxLengthLimit];
		var charsWritten = reader.CopyString(chars);
		return stringPool.GetOrAdd(chars[..charsWritten]);
	}

	public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return ReadStringOrGetFromPool(ref reader, StringPool);
	}

	public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
	{
	}
}