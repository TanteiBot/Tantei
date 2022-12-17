// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.List.Official.Base;

internal abstract class BaseListEntryStatus<TListStatus> where TListStatus : unmanaged, Enum
{
	[JsonPropertyName("status"), JsonConverter(typeof(JsonStringEnumConverter))]
	public required TListStatus Status { get; init; }

	[JsonPropertyName("score")]
	public required byte Score { get; init; }

	public abstract ulong ProgressedSubEntries { get; }

	public abstract bool IsReprogressing { get; }

	public abstract ulong ReprogressTimes { get; }

	[JsonPropertyName("tags")]
	public IReadOnlyList<string>? Tags { get; init; }

	[JsonPropertyName("comments")]
	public string? Comments { get; init; }

	[JsonPropertyName("updated_at")]
	public required DateTimeOffset UpdatedAt { get; init; }
}