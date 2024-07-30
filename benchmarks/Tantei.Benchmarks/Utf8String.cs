// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Text;
using System.Text.Unicode;
using BenchmarkDotNet.Attributes;

namespace Tantei.Benchmarks;

[SimpleJob]
[MemoryDiagnoser]
public class Utf8String
{
	private static ReadOnlySpan<byte> Bytes => @"\UtfBenchmarks\\UtfBenchmarks"u8;

	private const int MaxLengthLimit = 32;

	[Benchmark]
	public string EncodingClass()
	{
		scoped Span<char> chars = stackalloc char[MaxLengthLimit];
		var charsWritten = Encoding.UTF8.GetChars(Bytes, chars);
		return chars[..charsWritten].ToString();
	}

	[Benchmark]
	public string Utf8Class()
	{
		scoped Span<char> chars = stackalloc char[MaxLengthLimit];
		Utf8.ToUtf16(Bytes, chars, out _, out var charsWritten);
		return chars[..charsWritten].ToString();
	}
}