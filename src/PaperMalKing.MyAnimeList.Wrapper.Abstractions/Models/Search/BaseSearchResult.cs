// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.Common.Json;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Converters;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;

public abstract class BaseSearchResult
{
	[JsonPropertyName("id")]
	public required uint Id { get; init; }

	[JsonPropertyName("title")]
	[JsonConverter(typeof(ClearableStringPoolingJsonConverter))]
	public required string PrimaryTitle { get; init; }

	[JsonPropertyName("main_picture")]
	public Picture? Picture { get; init; }

	[JsonPropertyName("alternative_titles")]
	public AlternativeTitles? AlternativeTitles { get; init; }

	[JsonPropertyName("mean")]
	public double? Mean { get; init; }

	[JsonPropertyName("start_date")]
	[JsonConverter(typeof(DateOnlyFromMalConverter))]
	public DateOnly? StartDate { get; init; }

	[JsonPropertyName("num_list_users")]
	public required uint ListUserCount { get; init; }

	[JsonPropertyName("genres")]
	public IReadOnlyList<Genre>? Genres { get; init; }

	[JsonPropertyName("synopsis")]
	public string? Synopsis { get; init; }

	[JsonPropertyName("nsfw")]
	public NsfwCategory? Nsfw { get; init; }
}

public abstract class BaseSearchResult<TMediaType, TStatus> : BaseSearchResult
	where TMediaType : unmanaged, Enum
	where TStatus : unmanaged, Enum
{
	[JsonPropertyName("media_type")]
	public required TMediaType MediaType { get; init; }

	[JsonPropertyName("status")]
	public required TStatus Status { get; init; }
}
