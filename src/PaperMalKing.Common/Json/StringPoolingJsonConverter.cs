// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace PaperMalKing.Common.Json;

/// <summary>
/// This converter tries to pool string from incoming json if possible.
/// It is intended to work on small strings only (smaller or equal to <see cref="MaxLengthLimit"/> chars)
/// If input string is escaped, or length in bytes exceed 256, it isn't pooled, and is just allocated.
/// </summary>
public sealed class StringPoolingJsonConverter : JsonConverter<string>
{
	private static readonly HashSet<string> StringPool = new(StringComparer.Ordinal);

	public const int MaxLengthLimit = 256;

	public static string ReadStringOrGetFromPool(ref Utf8JsonReader reader) => ReadStringOrGetFromPool(ref reader, StringPool, readerWriterLock: null);

	/// <param name="reader">The reader.</param>
	/// <param name="stringPool">Pool to pool to or from value.</param>
	/// <param name="readerWriterLock">lock controlling access to string pool.</param>
	/// <remarks>
	/// Uses ref in order to not copy struct when invoking <see cref="Utf8JsonReader.GetString"/> in unhappy paths.
	/// </remarks>
	public static string ReadStringOrGetFromPool(scoped ref Utf8JsonReader reader, HashSet<string> stringPool, ReaderWriterLockSlim? readerWriterLock)
	{
		Debug.Assert(reader.TokenType == JsonTokenType.String, "Must be a string");
		if (reader.ValueIsEscaped || (reader.HasValueSequence ? reader.ValueSequence.Length : reader.ValueSpan.Length) > MaxLengthLimit)
		{
			return reader.GetString()!;
		}

		scoped Span<char> chars = stackalloc char[MaxLengthLimit];
		var charsWritten = reader.CopyString(chars);
		readerWriterLock?.EnterReadLock();
		var alternateLookup = stringPool.GetAlternateLookup<ReadOnlySpan<char>>();

		var searchValue = chars[..charsWritten];
		if (!alternateLookup.TryGetValue(searchValue, out var result))
		{
			readerWriterLock?.ExitReadLock();
			readerWriterLock?.EnterWriteLock();
			result = new string(searchValue);
			stringPool.Add(result);
			readerWriterLock?.ExitWriteLock();
		}
		else
		{
			readerWriterLock?.ExitReadLock();
		}

		return result;
	}

	public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return ReadStringOrGetFromPool(ref reader, StringPool, readerWriterLock: null);
	}

	public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
	{
	}
}