﻿// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using static PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums.TitleLanguage;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models;

public sealed class GenericName
{
	[JsonPropertyName("full")]
	public string? Full { get; init; }

	[JsonPropertyName("native")]
	public required string Native { get; init; }

	public string GetName(TitleLanguage language)
	{
		return (this.Full is not null) switch
		{
			true when language is ROMAJI_STYLISED or ENGLISH or ENGLISH_STYLISED or ROMAJI => this.Full,
			_ => this.Native,
		};
	}
}