// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models;

internal sealed class Connection<T>
{
	[JsonPropertyName("pageInfo")]
	public PageInfo? PageInfo { get; init; }

	[JsonPropertyName("values")]
	[SuppressMessage("Performance", "CA1819:Properties should not return arrays")]
	public required T[] Nodes { get; init; }

	public static readonly Connection<T> Empty = new()
	{
		PageInfo = new() { HasNextPage = false },
		Nodes = Array.Empty<T>()
	};
}