// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;

namespace PaperMalKing.Shikimori.Wrapper.Responses;

internal sealed class UserInfoResponse
{
	[JsonPropertyName("users")]
	public required UserInfo[] Users { get; init; }
}