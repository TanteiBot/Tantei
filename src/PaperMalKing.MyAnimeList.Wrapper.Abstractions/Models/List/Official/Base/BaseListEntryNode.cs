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
	[JsonRequired]
	public uint Id { get; internal set; }

	[JsonPropertyName("title")]
	[JsonConverter(typeof(ClearableStringPoolingJsonConverter))]
	[JsonRequired]
	public string Title { get; internal set; } = null!;

	[JsonPropertyName("main_picture")]
	public Picture? Picture { get; internal set; }

	[JsonPropertyName("synopsis")]
	public string? Synopsis { get; internal set; }

	[JsonPropertyName("genres")]
	public IReadOnlyList<Genre>? Genres { get; internal set; }

	[JsonPropertyName("media_type"), JsonConverter(typeof(JsonStringEnumConverter)),JsonRequired]
	public TMediaType MediaType { get; internal set; }

	[JsonPropertyName("status"), JsonConverter(typeof(JsonStringEnumConverter)),JsonRequired]
	public TStatus Status { get; internal set; }

	public abstract string Url { get; }

	public abstract uint TotalSubEntries { get; }
}