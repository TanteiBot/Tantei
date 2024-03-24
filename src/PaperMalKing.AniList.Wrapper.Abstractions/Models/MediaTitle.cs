// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Linq;
using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models;

public sealed class MediaTitle
{
	private readonly string?[] _titles = new string?[(int)(Enum.GetValuesAsUnderlyingType<TitleLanguage>() as TitleLanguage[])!.MaxBy(x => (byte)x) + 1];

	[JsonPropertyName("stylisedRomaji")]
	public string? StylisedRomaji
	{
		get => this._titles[(int)TitleLanguage.ROMAJI_STYLISED];
		init => this._titles[(int)TitleLanguage.ROMAJI_STYLISED] = value;
	}

	[JsonPropertyName("romaji")]
	public string? Romaji
	{
		get => this._titles[(int)TitleLanguage.ROMAJI];
		init => this._titles[(int)TitleLanguage.ROMAJI] = value;
	}

	[JsonPropertyName("stylisedEnglish")]
	public string? StylisedEnglish
	{
		get => this._titles[(int)TitleLanguage.ENGLISH_STYLISED];
		init => this._titles[(int)TitleLanguage.ENGLISH_STYLISED] = value;
	}

	[JsonPropertyName("english")]
	public string? English
	{
		get => this._titles[(int)TitleLanguage.ENGLISH];
		init => this._titles[(int)TitleLanguage.ENGLISH] = value;
	}

	[JsonPropertyName("stylisedNative")]
	public string? StylisedNative
	{
		get => this._titles[(int)TitleLanguage.NATIVE_STYLISED];
		init => this._titles[(int)TitleLanguage.NATIVE_STYLISED] = value;
	}

	[JsonPropertyName("native")]
	public string? Native
	{
		get => this._titles[(int)TitleLanguage.NATIVE];
		init => this._titles[(int)TitleLanguage.NATIVE] = value;
	}

	public string GetTitle(TitleLanguage titleLanguage)
	{
		var title = "";
		for (; titleLanguage != TitleLanguage.NATIVE - 1; titleLanguage--)
		{
			title = this._titles[(int)titleLanguage];
			if (!string.IsNullOrEmpty(title))
			{
				break;
			}
		}

		return title!;
	}
}