// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models;

public sealed class Connection<T>
{
	[JsonPropertyName("pageInfo")]
	public PageInfo? PageInfo { get; init; }

	[JsonPropertyName("values")]
	public required T[] Nodes { get; init; }

	public static readonly Connection<T> Empty = new()
	{
		PageInfo = PageInfo.FalseValue,
		Nodes = [],
	};
}