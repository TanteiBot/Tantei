// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models.Responses;

public sealed class InitialUserInfoResponse
{
	[JsonPropertyName("User")]
	public required User User { get; init; }
}