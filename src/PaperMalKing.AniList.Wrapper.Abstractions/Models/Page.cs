// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models;

public sealed class Page<T>
{
	[JsonPropertyName("pageInfo")]
	public PageInfo? PageInfo { get; init; }

	[JsonPropertyName("values")]
	public required IReadOnlyList<T> Values { get; init; }

	public static readonly Page<T> Empty = new()
	{
		PageInfo = PageInfo.FalseValue,
		Values = [],
	};
}