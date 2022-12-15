// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models;

internal sealed class FavouriteEntry : IEquatable<FavouriteEntry>
{
	[JsonIgnore]
	public string? GenericType { get; internal set; }

	[JsonIgnore]
	public string? SpecificType { get; internal set; } = null;

	[JsonPropertyName("id")]
	public ulong Id { get; init; }

	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonPropertyName("russian")]
	public string? RussianName { get; init; }

	public string? ImageUrl => Utils.GetImageUrl(this.GenericType!, this.Id);

	public string? Url => Utils.GetUrl(this.GenericType!, this.Id);

	public bool Equals(FavouriteEntry? other)
	{
		if (ReferenceEquals(null, other))
			return false;
		if (ReferenceEquals(this, other))
			return true;
		return this.GenericType == other.GenericType && this.Id == other.Id;
	}

	public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is FavouriteEntry other && this.Equals(other);

	public override int GetHashCode()
	{
		unchecked
		{
			return ((this.GenericType != null ? this.GenericType.GetHashCode(StringComparison.OrdinalIgnoreCase) : 0) * 397) ^ this.Id.GetHashCode();
		}
	}

	public static bool operator ==(FavouriteEntry? left, FavouriteEntry? right) => Equals(left, right);

	public static bool operator !=(FavouriteEntry? left, FavouriteEntry? right) => !Equals(left, right);
}