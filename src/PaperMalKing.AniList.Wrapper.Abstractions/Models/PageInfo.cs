// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.Common.Json;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models;

[JsonConverter(typeof(BoolWrapperConverter<PageInfo>))]
public sealed class PageInfo : IBoolWrapper<PageInfo>
{
	public bool HasNextPage { get; init; }

	public static PageInfo TrueValue { get; } = new()
	{
		HasNextPage = true
	};

	public static PageInfo FalseValue { get; } = new()
	{
		HasNextPage = false
	};
}