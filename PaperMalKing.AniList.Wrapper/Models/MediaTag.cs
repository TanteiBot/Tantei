// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models
{
	public sealed class MediaTag
	{
		[JsonPropertyName("name")]
		public string Name { get; init; } = null!;

		[JsonPropertyName("rank")]
		public byte Rank { get; init; }

		[JsonPropertyName("isMediaSpoiler")]
		public bool IsSpoiler { get; init; }
	}
}