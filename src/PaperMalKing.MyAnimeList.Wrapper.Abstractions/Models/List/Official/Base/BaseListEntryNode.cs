// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using PaperMalKing.Common.Json;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.Base;

public abstract class BaseListEntryNode<TMediaType, TStatus> where TMediaType : unmanaged, Enum where TStatus : unmanaged, Enum
{
	[JsonPropertyName("id")]
	public required uint Id { get; init; }

	[JsonPropertyName("title")]
	[JsonConverter(typeof(ClearableStringPoolingJsonConverter))]
	public required string Title { get; init; }

	[JsonPropertyName("main_picture")]
	public Picture? Picture { get; init; }

	[JsonPropertyName("synopsis")]
	public string? Synopsis { get; init; }

	[JsonPropertyName("genres")]
	public IReadOnlyList<Genre>? Genres { get; init; }

	[JsonPropertyName("media_type"), JsonConverter(typeof(JsonStringEnumConverter))]
	public required TMediaType MediaType { get; init; }

	[JsonPropertyName("status"), JsonConverter(typeof(JsonStringEnumConverter))]
	public required TStatus Status { get; init; }

	public abstract string Url { get; }

	public abstract uint TotalSubEntries { get; }
}