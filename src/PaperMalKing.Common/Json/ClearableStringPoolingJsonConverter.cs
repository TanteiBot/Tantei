// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;

namespace PaperMalKing.Common.Json;

/// <summary>
/// This string converter should be applied to strings, that are expected to not be present in incoming data in some time
/// For example, media titles, that are constantly returned by update provider, but can be not present in response if user updated their list.
/// And so on.
/// As well as <see cref="StringPoolingJsonConverter"/> it should be applied on small strings only, see docs on <see cref="StringPoolingJsonConverter"/>.
/// </summary>
public sealed class ClearableStringPoolingJsonConverter : JsonConverter<string>
{
	private static readonly HashSet<string> StringPool = new(StringComparer.Ordinal);

	private static readonly ReaderWriterLockSlim ReaderWriterLock = new(LockRecursionPolicy.NoRecursion);

	[SuppressMessage("Performance", "CA1823:Avoid unused private fields", Justification = "We store it just in case")]
	[SuppressMessage("Roslynator", "RCS1213:Remove unused member declaration.", Justification = "We store it just in case")]
	[SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "We store it just in case")]
	private static readonly Timer Timer = new(static _ =>
	{
		ReaderWriterLock.EnterWriteLock();
		StringPool.Clear();
		ReaderWriterLock.ExitWriteLock();
	},
	state: null,
	dueTime: TimeSpan.Zero,
	TimeSpan.FromHours(3));

	public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		return StringPoolingJsonConverter.ReadStringOrGetFromPool(ref reader, StringPool, ReaderWriterLock);
	}

	public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
	{
	}
}