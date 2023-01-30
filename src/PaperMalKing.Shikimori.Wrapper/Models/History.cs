// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models;

internal sealed class History
{
	[JsonPropertyName("id")]
	public uint Id { get; init; }

	[JsonPropertyName("created_at")]
	public DateTimeOffset CreatedAt { get; init; }

	[JsonPropertyName("description")]
	public required string Description { get; init; }

	[JsonPropertyName("target")]
	public HistoryTarget? Target { get; init; }
}