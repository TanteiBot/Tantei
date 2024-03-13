// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.Common.Json;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models;

[JsonConverter(typeof(BoolWrapperConverter<MediaListTypeOptions>))]
public sealed class MediaListTypeOptions : IBoolWrapper<MediaListTypeOptions>
{
	public static MediaListTypeOptions TrueValue { get; } = new()
	{
		IsAdvancedScoringEnabled = true,
	};

	public static MediaListTypeOptions FalseValue { get; } = new()
	{
		IsAdvancedScoringEnabled = false,
	};

	[JsonPropertyName("advancedScoringEnabled")]
	public bool IsAdvancedScoringEnabled { get; init; }
}