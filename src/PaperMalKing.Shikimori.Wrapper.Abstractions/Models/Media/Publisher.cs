// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

public sealed class Publisher
{
	[JsonPropertyName("id")]
	[JsonRequired]
	public uint Id { get; internal set; }

	// Dont pull publishers name, there are ~1000 of them
	// They aren't available in other providers as of yet, so there wont be an overlap
	// And we dont expect retrieving Publishers in Fast Path, when no updates were found for user
	[JsonPropertyName("name")]
	[JsonRequired]
	public string Name { get; internal set; } = null!;

	public string Url => Utils.GetUrl("mangas/publisher", this.Id);
}