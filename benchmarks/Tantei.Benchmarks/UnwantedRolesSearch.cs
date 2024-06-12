// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using BenchmarkDotNet.Attributes;

namespace Tantei.Benchmarks;

[SimpleJob]
[MemoryDiagnoser]
[SuppressMessage("Minor Code Smell", "S3267:Loops should be simplified with \"LINQ\" expressions", Justification = "Perf is important")]
public class UnwantedRolesSearch
{
	private static readonly string[] IgnoredContainsRoles = ["Assist", "Edit", "Insert", "Consultant", "Cooperation"];

	private static readonly string[] IgnoredStartWithRoles =
	[
		"Touch-Up",
		"Touch Up",
		"Illustrat",
		"Collaborat",
		"Color",
		"Digital Coloring",
		"Cooking Supervisor",
		"Letter",   // Letterer and Lettering
		"Translat", // Translator and Translation
	];

	private static readonly SearchValues<string> SVContains = SearchValues.Create(IgnoredContainsRoles, StringComparison.OrdinalIgnoreCase);

	private static readonly SearchValues<string> SVStartsWith = SearchValues.Create(IgnoredStartWithRoles, StringComparison.OrdinalIgnoreCase);

	[Params("Assistant", "Editor", "Cooperationist", "Translator", "Letterer", "Color", "Director")]
	public string Role { get; set; } = null!;

	[Benchmark]
	[BenchmarkCategory("Contains")]
	public bool ArrayContains()
	{
		foreach (var icr in IgnoredContainsRoles)
		{
			if (this.Role.Contains(icr, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
		}

		return true;
	}

	[Benchmark]
	[BenchmarkCategory("Contains")]
	public bool SearchValuesContains()
	{
		return !this.Role.AsSpan().ContainsAny(SVContains);
	}

	[Benchmark]
	[BenchmarkCategory("StartsWith")]
	public bool ArrayStartsWith()
	{
		foreach (var icr in IgnoredStartWithRoles)
		{
			if (this.Role.StartsWith(icr, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
		}

		return true;
	}

	[Benchmark]
	[BenchmarkCategory("StartsWith")]
	public bool SearchValuesStartsWith()
	{
		return !this.Role.AsSpan().ContainsAny(SVStartsWith);
	}
}