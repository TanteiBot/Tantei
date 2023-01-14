// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Json;

namespace PaperMalKing.AniList.Wrapper.Models;

[JsonConverter(typeof(PageInfoConverter))]
internal sealed class PageInfo
{
	public bool HasNextPage { get; init; }
}