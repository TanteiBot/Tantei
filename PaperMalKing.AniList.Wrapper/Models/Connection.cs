// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models;

public sealed class Connection<T>
{
	[JsonPropertyName("pageInfo")]
	public PageInfo PageInfo { get; init; } = null!;

	[JsonPropertyName("values")]
	#pragma warning disable CA1819
	public T[] Nodes { get; init; } = null!;
	#pragma warning restore CA1819

	public static readonly Connection<T> Empty = new()
	{
		PageInfo = new() {HasNextPage = false},
		Nodes = Array.Empty<T>()
	};
}