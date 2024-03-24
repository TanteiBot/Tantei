// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Interfaces;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models;

public sealed class IdentifiableFavourite : IIdentifiable, IEquatable<IdentifiableFavourite>
{
	[JsonPropertyName("id")]
	public uint Id { get; init; }

	[JsonIgnore]
	public FavouriteType Type { get; set; }

	public bool Equals(IdentifiableFavourite? other)
	{
		return other is not null && (ReferenceEquals(this, other) || (this.Id == other.Id && this.Type == other.Type));
	}

	public override bool Equals(object? obj)
	{
		return ReferenceEquals(this, obj) || (obj is IdentifiableFavourite other && this.Equals(other));
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(this.Id, (int)this.Type);
	}
}