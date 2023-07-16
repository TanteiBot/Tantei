// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models;

public sealed class UserOptions
{
	[JsonPropertyName("titleLanguage")]
	public TitleLanguage TitleLanguage { get; init; }
}