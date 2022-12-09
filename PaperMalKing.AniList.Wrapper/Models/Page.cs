// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models;

public sealed class Page<T>
{
	[JsonPropertyName("pageInfo")]
	public PageInfo PageInfo { get; init; } = null!;

	[JsonPropertyName("values")]
	public IReadOnlyList<T> Values { get; init; } = null!;

	public static readonly Page<T> Empty = new()
	{
		PageInfo = new() {HasNextPage = false},
		Values = Array.Empty<T>()
	};
}