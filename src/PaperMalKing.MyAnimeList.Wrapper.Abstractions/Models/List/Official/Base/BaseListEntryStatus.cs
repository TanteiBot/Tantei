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

	[JsonPropertyName("status"), JsonConverter(typeof(JsonStringEnumConverter)), JsonRequired]
	public TListStatus Status { get; internal set; }

	[JsonPropertyName("score")]
	[JsonRequired]
	public byte Score { get; internal set; }

	public abstract ulong ProgressedSubEntries { get; }

	public abstract bool IsReprogressing { get; }

	public abstract ulong ReprogressTimes { get; }

	[JsonPropertyName("tags")]
	public IReadOnlyList<string>? Tags { get; internal set; }

	[JsonPropertyName("comments")]
	public string? Comments { get; internal set; }

	[JsonPropertyName("updated_at")]
	[JsonRequired]
	public DateTimeOffset UpdatedAt { get; internal set; }

	[JsonPropertyName("start_date"), JsonConverter(typeof(DateOnlyFromMalConverter))]
	public DateOnly? StartDate { get; internal set; }

	[JsonPropertyName("finish_date"), JsonConverter(typeof(DateOnlyFromMalConverter))]
	public DateOnly? FinishDate { get; internal set; }
}