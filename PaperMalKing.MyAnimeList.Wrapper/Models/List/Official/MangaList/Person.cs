// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.List.Official.MangaList;

internal sealed class Person
{
	[JsonPropertyName("id")]
	public required uint Id { get; init; }

	[JsonPropertyName("first_name")]
	public string? FirstName { get; init; }

	[JsonPropertyName("last_name")]
	public string? LastName { get; init; }
}