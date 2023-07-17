// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Converters;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.Base;

public abstract class BaseListEntryStatus<TListStatus> where TListStatus : unmanaged, Enum
{
	public byte GetStatusAsUnderlyingType()
	{
		Debug.Assert(Enum.GetUnderlyingType(typeof(TListStatus)) == typeof(byte));
		var status = this.Status;
		return Unsafe.As<TListStatus, byte>(ref status);
	}

	[JsonPropertyName("status")]
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

	[JsonPropertyName("start_date"), JsonConverter(typeof(DateOnlyFromMalConverter))]
	public DateOnly? StartDate { get; init; }

	[JsonPropertyName("finish_date"), JsonConverter(typeof(DateOnlyFromMalConverter))]
	public DateOnly? FinishDate { get; init; }
}