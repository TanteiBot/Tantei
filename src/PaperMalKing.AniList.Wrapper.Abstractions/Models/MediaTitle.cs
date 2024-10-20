// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Linq;
using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models;

public sealed class MediaTitle
{
	private static readonly int MaxTitles = (int)(Enum.GetValuesAsUnderlyingType<TitleLanguage>() as TitleLanguage[])!.MaxBy(static x => (byte)x) + 1;
	private readonly string?[] _titles = new string?[MaxTitles];

	[JsonPropertyName("stylisedRomaji")]
	public string? StylisedRomaji
	{
		get => this._titles[(int)TitleLanguage.RomajiStylised];
		init => this._titles[(int)TitleLanguage.RomajiStylised] = value;
	}

	[JsonPropertyName("romaji")]
	public string? Romaji
	{
		get => this._titles[(int)TitleLanguage.Romaji];
		init => this._titles[(int)TitleLanguage.Romaji] = value;
	}

	[JsonPropertyName("stylisedEnglish")]
	public string? StylisedEnglish
	{
		get => this._titles[(int)TitleLanguage.EnglishStylised];
		init => this._titles[(int)TitleLanguage.EnglishStylised] = value;
	}

	[JsonPropertyName("english")]
	public string? English
	{
		get => this._titles[(int)TitleLanguage.English];
		init => this._titles[(int)TitleLanguage.English] = value;
	}

	[JsonPropertyName("stylisedNative")]
	public string? StylisedNative
	{
		get => this._titles[(int)TitleLanguage.NativeStylised];
		init => this._titles[(int)TitleLanguage.NativeStylised] = value;
	}

	[JsonPropertyName("native")]
	public string? Native
	{
		get => this._titles[(int)TitleLanguage.Native];
		init => this._titles[(int)TitleLanguage.Native] = value;
	}

	public string GetTitle(TitleLanguage titleLanguage)
	{
		var title = "";
		for (; titleLanguage != TitleLanguage.Native - 1; titleLanguage--)
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