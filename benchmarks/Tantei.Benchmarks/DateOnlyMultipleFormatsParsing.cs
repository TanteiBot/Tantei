// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Globalization;
using BenchmarkDotNet.Attributes;

namespace Tantei.Benchmarks;

[SimpleJob]
[MemoryDiagnoser]
public class DateOnlyMultipleFormatsParsing
{
	private static readonly string[] Formats = ["yyyy-MM-dd", "yyyy-MM", "yyyy", "yyyy-M-dd", "yyyy-M-d", "yyyy-M"];

	[Params("2024-11-01", "2024-5", "2024", "2024-05")]
	public string Input { get; set; } = null!;

	[Benchmark]
	public DateOnly Loop()
	{
		for (var i = 0; i < Formats.Length; i++)
		{
			if (DateOnly.TryParseExact(this.Input, Formats[i], DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out var result))
			{
				return result;
			}
		}

		throw new InvalidOperationException("Not Found");
	}

	[Benchmark]
	public DateOnly CollectionOverload()
	{
		if (DateOnly.TryParseExact(this.Input, Formats, DateTimeFormatInfo.InvariantInfo, DateTimeStyles.None, out var result))
		{
			return result;
		}

		throw new InvalidOperationException("Not Found");
	}
}