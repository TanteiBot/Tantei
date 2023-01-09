// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Enums;

namespace PaperMalKing.AniList.Wrapper.Models;

internal sealed class GenericName
{
	[JsonPropertyName("full")]
	public string? Full { get; init; }

	[JsonPropertyName("native")]
	public required string Native { get; init; }

	public string GetName(TitleLanguage language)
	{
		return language switch
		{
			TitleLanguage.ROMAJI_STYLISED when this.Full != null => this.Full,
			TitleLanguage.ENGLISH when this.Full != null => this.Full,
			TitleLanguage.ENGLISH_STYLISED when this.Full != null => this.Full,
			TitleLanguage.ROMAJI when this.Full != null => this.Full,
			_ => this.Native
		};
	}
}