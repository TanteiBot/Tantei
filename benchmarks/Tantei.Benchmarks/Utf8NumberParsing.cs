// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Buffers.Text;
using System.Globalization;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Tantei.Benchmarks;

[SimpleJob]
[MemoryDiagnoser]
public class Utf8NumberParsing
{
	private static ReadOnlySpan<byte> Bytes => "122345"u8;

	private const int MaxLengthLimit = 10;

	[Benchmark]
	public int EncodingClass()
	{
		scoped Span<char> chars = stackalloc char[MaxLengthLimit];
		var charsWritten = Encoding.UTF8.GetChars(Bytes, chars);
		var hexValue = chars[..charsWritten];
		return int.Parse(hexValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
	}

	[Benchmark]
	public int Utf8Class()
	{
		_ = Utf8Parser.TryParse(Bytes, out int value, out _, 'X');
		return value;
	}
}